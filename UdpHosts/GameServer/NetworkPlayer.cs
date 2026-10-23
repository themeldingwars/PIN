using System;
using System.Net;
using System.Numerics;
using System.Threading;
using AeroMessages.GSS;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Controller;
using AeroMessages.GSS.Character.Event;
using AeroMessages.Matrix;
using GameServer.Data;
using GameServer.Protocol;
using GameServer.StaticDB.Records.customdata;
using GameServer.Systems.Combat;
using Serilog;
using CharacterEntity = GameServer.Entities.Character.CharacterEntity;
using ClientStatus = GameServer.Protocol.ClientStatus;

namespace GameServer;

public class NetworkPlayer : NetworkClient, INetworkPlayer
{
    private static readonly PersonalFactionStance DefaultPlayerFactionStance = new(new PersonalFactionStanceData
    {
        Friendly = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = [0x09, 0x0e, 0x5d, 0xff, 0x5f, 0x08, 0x00, 0x00] },
        Hostile = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = [0xf2, 0x00, 0x20, 0x00, 0x00, 0xf2, 0x00, 0x00] },
    });

    public NetworkPlayer(IPEndPoint endPoint, uint socketId, ILogger logger)
        : base(endPoint, socketId, logger)
    {
        CharacterEntity = null;
        Status = IPlayer.PlayerStatus.Connecting;
        ConnectedAt = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public ulong PlayerId { get; set; }
    public ulong CharacterId { get; set; }
    public CharacterEntity CharacterEntity { get; set; }
    public IPlayer.PlayerStatus Status { get; set; }
    public PlayerPreferences Preferences { get; private set; }
    public Zone CurrentZone { get; private set; }
    public uint CurrentOutpostId { get; private set; }
    public uint LastRequestedUpdate { get; set; }
    public uint RequestedClientTime { get; set; }
    public bool FirstUpdateRequested { get; set; }
    public ulong SteamUserId { get; set; }
    public bool OwnCharacterViewsRequested { get; set; }
    public CharacterInventory Inventory { get; set; }
    public CharacterUnlocks Unlocks { get; set; }
    public CharacterBattleframes Battleframes { get; set; }
    public uint ConnectedAt { get; }
    public bool CanReceiveGSS => (Status.Equals(IPlayer.PlayerStatus.Playing) || Status.Equals(IPlayer.PlayerStatus.Loading)) && NetClientStatus.Equals(ClientStatus.Connected);

    public void Init(IShard shard)
    {
        Init(this, shard, shard);
        Preferences = new PlayerPreferences();
    }

    public void EnterZoneAck()
    {
        OwnCharacterViewsRequested = false;
        AssignedShard.EntityMan.Add(CharacterEntity.EntityId, CharacterEntity);
        AssignedShard.EntityMan.OnPlayerJoin(this);
    }

    public void ExitZoneAck()
    {
        LeaveZone();
    }

    public void Respawn()
    {
        var outpostId = FindClosestAvailableOutpost(CurrentZone, CurrentOutpostId);
        var spawnPoint = outpostId == 0 ?
                             new SpawnPoint { Position = CurrentZone.POIs["spawn"] }
                             : AssignedShard.Outposts[CurrentZone.ID][outpostId].RandomSpawnPoint;

        CharacterEntity.PositionAtSpawnPoint(spawnPoint);
        CharacterEntity.SetSpawnTime(AssignedShard.CurrentTime);
        var forcedMove = new ForcedMovement
        {
            Data = new ForcedMovementData
            {
                Type = 1,
                CommandId = 0,
                HaveRelativeTo = 0,
                Params1 = new ForcedMovementType1Params { Position = spawnPoint.Position, Direction = CharacterEntity.AimDirection, Velocity = Vector3.Zero, Time = AssignedShard.CurrentTime + 1 }
            },
            ShortTime = AssignedShard.CurrentShortTime
        };
        NetChannels[ChannelType.ReliableGss].SendMessage(forcedMove, CharacterEntity.EntityId);

        var respawnMsg = new Respawned { ShortTime = AssignedShard.CurrentShortTime, ClearProjectiles = 0, Unk2 = 0 };
        NetChannels[ChannelType.ReliableGss].SendMessage(respawnMsg, CharacterEntity.EntityId);

        var baseController = CharacterEntity.Character_BaseController;

        // Update 1
        CharacterEntity.SetSpawnTime(AssignedShard.CurrentTime);
        CharacterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Respawning, AssignedShard.CurrentTime);
        CharacterEntity.SetSpawnPose();
        baseController.RespawnTimesProp = new RespawnTimesData();
        baseController.RespawnTimesProp = null; // Make the field dirty so we send clear because we probably should send clear. At some point investigaste if this is neccessary.
        baseController.TimedDailyRewardProp = new TimedDailyRewardData { State = TokenMachineState.Rolled, MaxRolls = 1, CountdownToTime = AssignedShard.CurrentTime };
        NetChannels[ChannelType.ReliableGss].SendChanges(baseController, CharacterEntity.EntityId);

        // Update 2
        CharacterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Living, AssignedShard.CurrentTime + 1);
        baseController.GibVisualsIdProp = new GibVisuals { Id = 0, Time = AssignedShard.CurrentTime + 1 };
        baseController.RespawnTimesProp = new RespawnTimesData(); // Shake it up
        baseController.RespawnTimesProp = null; // It's dirt
        baseController.CurrentHealthProp = CharacterEntity.MaxHealth.Value;
        baseController.MaxHealthProp = new MaxVital { Value = CharacterEntity.MaxHealth.Value, Time = AssignedShard.CurrentTime };
        baseController.CurrentShieldsProp = 0;
        baseController.ZoneUnlocksProp = 0xFFFFFFFFFFFFFFFFUL;
        baseController.RegionUnlocksProp = 0xFFFFFFFFFFFFFFFFUL;
        CharacterEntity.SetPersonalFactionStance(DefaultPlayerFactionStance);
        NetChannels[ChannelType.ReliableGss].SendChanges(baseController, CharacterEntity.EntityId);

        // A respawn starts a new life: no status effect, held ability or half-spent magazine from the old one
        // carries over, and the frame's default abilities run again. This also keeps the jetpack effects below
        // from stacking on every respawn.
        CharacterEntity.ResetCombatState("respawn");

        // Hack to add in jetpack fx until we hook up item effects
        CharacterEntity.AddEffect(AssignedShard.Abilities.Factory.LoadEffect(986), new Systems.Aptitude.Context(AssignedShard, CharacterEntity)
        {
            InitTime = AssignedShard.CurrentTime,
        });
        CharacterEntity.AddEffect(AssignedShard.Abilities.Factory.LoadEffect(472), new Systems.Aptitude.Context(AssignedShard, CharacterEntity)
        {
            InitTime = AssignedShard.CurrentTime,
        });

        CharacterEntity.LogSpeedSnapshot("after respawn");

        var combatController = new CombatController
        {
            CombatTimer_0Prop = AssignedShard.CurrentTime,
        };
        NetChannels[ChannelType.ReliableGss].SendChanges(combatController, CharacterEntity.EntityId);

        AssignedShard.Physics.UpdateEntity(CharacterEntity);
        CharacterEntity.Alive = true; // Accept MovementInputs only after Respawn
        AssignedShard.CharacterLifecycle.OnCharacterCreated(CharacterEntity);
    }

    public void Ready()
    {
        Status = IPlayer.PlayerStatus.Playing;
    }

    public void RequestRevive(INetworkPlayer reviver)
    {
        if (reviver == null || CharacterEntity == null)
        {
            return;
        }

        AssignedShard.CharacterLifecycle.TryRevive(CharacterEntity, reviver.CharacterEntity);
    }

    public void Jump()
    {
        CharacterEntity.TimeSinceLastJump = -1;
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        switch (Status)
        {
            // TODO: Implement FSM here to move player thru log in process to connecting to a shard to playing
            case IPlayer.PlayerStatus.Connected:
                Status = IPlayer.PlayerStatus.LoggingIn;
                break;
            case IPlayer.PlayerStatus.Loading:
                break;
            case IPlayer.PlayerStatus.Playing:
                {
                    break;
                }
        }
    }

    public uint FindClosestAvailableOutpost(Zone zone, uint targetOutpostId = 0)
    {
        bool haveOutposts = AssignedShard.Outposts.TryGetValue(zone.ID, out var outposts);
        if (!haveOutposts)
        {
            return 0;
        }
        else if (targetOutpostId == 0)
        {
            return zone.DefaultOutpostId;
        }

        var targetOutpost = outposts[targetOutpostId];

        if (!targetOutpost.IsCapturedByHostiles)
        {
            return targetOutpostId;
        }

        Vector3 sourcePosition = targetOutpost.Outpost_ObserverView.PositionProp;

        var minDistance = Vector3.DistanceSquared(sourcePosition, zone.POIs["spawn"]);
        var closestOutpostId = zone.DefaultOutpostId;

        foreach (var outpost in outposts)
        {
            if (outpost.Value.IsCapturedByHostiles)
            {
                continue;
            }

            var distance = Vector3.DistanceSquared(sourcePosition, outpost.Value.Outpost_ObserverView.PositionProp);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestOutpostId = outpost.Key;
            }
        }

        return closestOutpostId;
    }

    public void EnterZone(Zone zone, uint outpostId = 0)
    {
        var spawnPoint = outpostId == 0
                             ? new SpawnPoint { Position = zone.POIs["spawn"] }
                             : AssignedShard.Outposts[zone.ID][outpostId].RandomSpawnPoint;

        // Ensure character entity is placed at the respawn point
        CharacterEntity.PositionAtSpawnPoint(spawnPoint);
        CharacterEntity.SetSpawnPose();

        CurrentZone = zone;
        CurrentOutpostId = outpostId;

        var msg = new EnterZone
        {
            InstanceId = AssignedShard.InstanceId,
            ZoneId = CurrentZone.ID,
            ZoneTimestamp = AssignedShard.Physics.ZoneFileTimestamp ?? (long)CurrentZone.Timestamp,
            ZoneFlags = 0,
            ZoneOwner = "r5_exec",
            StreamingProtocol = 0x4c5f,
            SvnRevision = 0x0c9f5,
            HotfixLevel = 0,
            MatchId = 0,
            Unk2 = 0,
            SimulationSeedMs = 0x63e2db5e,
            ZoneName = CurrentZone.Name,
            HaveDevZoneInfo = 0,
            ZoneTimeSyncInfo = new ZoneTimeSyncData { FictionDateTimeOffsetMicros = 0, DayLengthFactor = 12.0F, DayPhaseOffset = 0.896445870399F },
            GameClockInfo = new GameClockInfoData
            {
                EarliestUnixMicros = 1478970208392232,
                SimulationSeedUnixMicros = 1478774752697322,
                Timescale = 1.0,
                PausedAtMicros = 0,
                ClockOffsetMicros = 0,
                Paused = 0
            },
            SpectatorModeFlag = 0
        };

        NetChannels[ChannelType.Matrix].SendMessage(msg);

        Status = IPlayer.PlayerStatus.Loading;
    }

    private void LeaveZone()
    {
        AssignedShard.EntityMan.OnPlayerLeft(this);
        AssignedShard.EntityMan.Remove(CharacterEntity);
    }
}