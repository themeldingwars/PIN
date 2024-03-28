using System;
using System.Net;
using System.Numerics;
using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Controller;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.Matrix.V25;
using GameServer.Data;
using GameServer.Data.SDB.Records.customdata;
using GameServer.GRPC;
using GameServer.Test;
using GrpcGameServerAPIClient;
using Serilog;
using CharacterEntity = GameServer.Entities.Character.CharacterEntity;
using Loadout = AeroMessages.GSS.V66.Character.Event.Loadout;

namespace GameServer;

public class NetworkPlayer : NetworkClient, INetworkPlayer
{
    private double _lastKeyFrame;

    public NetworkPlayer(IPEndPoint endPoint, uint socketId, ILogger logger)
        : base(endPoint, socketId, logger)
    {
        CharacterEntity = null;
        Status = IPlayer.PlayerStatus.Connecting;
        ConnectedAt = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public ulong PlayerId { get; private set; }
    public ulong CharacterId { get; private set; }
    public CharacterEntity CharacterEntity { get; private set; }
    public IPlayer.PlayerStatus Status { get; private set; }
    public Zone CurrentZone { get; private set; }
    public uint CurrentOutpostId { get; private set; }
    public uint LastRequestedUpdate { get; set; }
    public uint RequestedClientTime { get; set; }
    public bool FirstUpdateRequested { get; set; }
    public ulong SteamUserId { get; set; }
    public CharacterInventory Inventory { get; set; }
    public uint ConnectedAt { get; }

    public void Init(IShard shard)
    {
        Init(this, shard, shard);
    }

    public async void Login(ulong characterId)
    {
        PlayerId = 0x4658281c142e9f00ul;
        var guid = characterId & 0xffffffffffffff00;
        CharacterId = guid;

        // Don't crash if they are already logged in
        AssignedShard.Entities.TryGetValue(CharacterId, out var existing);
        if (existing != null)
        {
            Console.WriteLine($"Closing login because entity with this id is already zoned in");
            var resp = new AeroMessages.Control.CloseConnection { Unk = new byte[] { 0, 0, 0, 0 } };
            NetChannels[ChannelType.Control].SendIAero(resp);
            return;
        }

        // Begin setting up player character
        CharacterEntity = new CharacterEntity(AssignedShard, guid);

        // Try to get remote character data
        CharacterAndBattleframeVisuals remoteData = null;
        try
        {
            remoteData = await GRPCService.GetCharacterAndBattleframeVisualsAsync((long)characterId);
        }
        catch
        {
            Console.WriteLine($"Could not get character over GRPC, will use fallback");
        }

        // Load inventory so we get loadouts
        Inventory = new CharacterInventory(AssignedShard, this, CharacterEntity);
        Inventory.LoadHardcodedInventory();

        // Use remote data or fallback to setup character
        bool useRemoteData = true;
        uint loadoutId;
        if (remoteData != null && useRemoteData)
        {
            CharacterEntity.LoadRemote(remoteData);

            // Todo: load inventory from db so we can use those loadouts
            loadoutId = Inventory.GetLoadoutIdForChassis(remoteData.CharacterInfo.CurrentBattleframeSDBId);
        }
        else
        {
            CharacterEntity.Load(HardcodedCharacterData.FallbackData);
            loadoutId = Inventory.GetLoadoutIdForChassis(76331);
        }

        var loadoutRefData = Inventory.GetLoadoutReferenceData(loadoutId);
        var loadout = new CharacterLoadout(loadoutRefData);
        CharacterEntity.ApplyLoadout(loadout);

        CharacterEntity.SetControllingPlayer(this);
        CharacterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Spawning, AssignedShard.CurrentTime);
        Status = IPlayer.PlayerStatus.LoggedIn;

        // WelcomeToTheMatrix
        var wel = new WelcomeToTheMatrix { PlayerID = PlayerId, Unk1 = Array.Empty<byte>(), Unk2 = Array.Empty<byte>() };
        NetChannels[ChannelType.Matrix].SendIAero(wel);

        Zone zone;
        uint zoneId;
        uint outpostId;

        if (remoteData != null)
        {
            zoneId = remoteData.CharacterInfo.LastZoneId;
            zone = DataUtils.GetZone(zoneId);
            outpostId = FindClosestAvailableOutpost(zone, remoteData.CharacterInfo.LastOutpostId);
        }
        else
        {
            zoneId = (uint)(characterId & 0x000000000000ffff);
            zone = DataUtils.GetZone(zoneId);
            outpostId = zone.DefaultOutpostId;
        }

        Logger.Verbose("Zone {0} Outpost {1}", zoneId, outpostId);

        EnterZone(zone, outpostId);
    }

    public void EnterZoneAck()
    {
        AssignedShard.EntityMan.Add(CharacterEntity.EntityId, CharacterEntity);
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
                Unk1 = 0,
                HaveUnk2 = 0,
                Params1 = new ForcedMovementType1Params { Position = spawnPoint.Position, Direction = CharacterEntity.AimDirection, Velocity = Vector3.Zero, Time = AssignedShard.CurrentTime + 1 }
            },
            ShortTime = AssignedShard.CurrentShortTime
        };
        NetChannels[ChannelType.ReliableGss].SendIAero(forcedMove, CharacterEntity.EntityId);

        var respawnMsg = new Respawned { ShortTime = AssignedShard.CurrentShortTime, Unk1 = 0, Unk2 = 0 };
        NetChannels[ChannelType.ReliableGss].SendIAero(respawnMsg, CharacterEntity.EntityId);

        var baseController = CharacterEntity.Character_BaseController;

        // Update 1
        CharacterEntity.SetSpawnTime(AssignedShard.CurrentTime);
        CharacterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Respawning, AssignedShard.CurrentTime);
        CharacterEntity.SetSpawnPose();
        baseController.RespawnTimesProp = new RespawnTimesData(); 
        baseController.RespawnTimesProp = null; // Make the field dirty so we send clear because we probably should send clear. At some point investigaste if this is neccessary.
        baseController.TimedDailyRewardProp = new TimedDailyRewardData { State = TimedDailyRewardData.TimedDailyRewardState.ROLLED, MaxRolls = 1, CountdownToTime = AssignedShard.CurrentTime };
        NetChannels[ChannelType.ReliableGss].SendIAeroChanges(baseController, CharacterEntity.EntityId);

        // Update 2
        CharacterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Living, AssignedShard.CurrentTime + 1);
        baseController.GibVisualsIdProp = new GibVisuals { Id = 0, Time = AssignedShard.CurrentTime + 1 };
        baseController.RespawnTimesProp = new RespawnTimesData(); // Shake it up
        baseController.RespawnTimesProp = null; // It's dirt
        baseController.CurrentHealthProp = HardcodedCharacterData.MaxHealth;
        baseController.MaxHealthProp = new MaxVital { Value = HardcodedCharacterData.MaxHealth, Time = AssignedShard.CurrentTime };
        baseController.CurrentShieldsProp = 0;
        baseController.ZoneUnlocksProp = 0xFFFFFFFFFFFFFFFFUL;
        baseController.RegionUnlocksProp = 0xFFFFFFFFFFFFFFFFUL;
        baseController.PersonalFactionStanceProp = new PersonalFactionStanceData
        {
            Unk1 = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = new byte[] { 0x09, 0x0e, 0x5d, 0xff, 0x5f, 0x08, 0x00, 0x00 } },
            Unk2 = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = new byte[] { 0xf2, 0x00, 0x20, 0x00, 0x00, 0xf2, 0x00, 0x00 } }
        };
        NetChannels[ChannelType.ReliableGss].SendIAeroChanges(baseController, CharacterEntity.EntityId);

        // Hack to add in jetpack fx until we hook up item effects
        CharacterEntity.AddEffect(AssignedShard.Abilities.Factory.LoadEffect(986), new Aptitude.Context(AssignedShard, CharacterEntity)
        {
            InitTime = AssignedShard.CurrentTime,
        });
        CharacterEntity.AddEffect(AssignedShard.Abilities.Factory.LoadEffect(472), new Aptitude.Context(AssignedShard, CharacterEntity)
        {
            InitTime = AssignedShard.CurrentTime,
        });

        var combatController = new CombatController
        {
            CombatTimer_0Prop = AssignedShard.CurrentTime,
        };
        NetChannels[ChannelType.ReliableGss].SendIAeroChanges(combatController, CharacterEntity.EntityId);

        // InventoryUpdate
        Inventory.SendFullInventory();
        Inventory.EnablePartialUpdates = true;

        CharacterEntity.Alive = true; // Accept MovementInputs only after Respawn
    }

    public void Ready()
    {
        Status = IPlayer.PlayerStatus.Playing;
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
                    if (AssignedShard.CurrentTime - _lastKeyFrame > 0.5)
                    {
                        // NetChannels[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(this, this), this.InstanceID, msgEnumType: typeof(Enums.GSS.Character.Events));
                    }

                    break;
                }
        }
    }

    public uint FindClosestAvailableOutpost(Zone zone, uint targetOutpostId = 0)
    {
        if (targetOutpostId == 0)
        {
            return zone.DefaultOutpostId;
        }

        var targetOutpost = AssignedShard.Outposts[zone.ID][targetOutpostId];

        if (!targetOutpost.IsCapturedByHostiles)
        {
            return targetOutpostId;
        }

        Vector3 sourcePosition = targetOutpost.Outpost_ObserverView.PositionProp;

        var minDistance = Vector3.DistanceSquared(sourcePosition, zone.POIs["spawn"]);
        var closestOutpostId = zone.DefaultOutpostId;

        foreach (var outpost in AssignedShard.Outposts[zone.ID])
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

    private void EnterZone(Zone z, uint outpostId = 0)
    {
        var spawnPoint = outpostId == 0
                             ? new SpawnPoint { Position = z.POIs["spawn"] }
                             : AssignedShard.Outposts[z.ID][outpostId].RandomSpawnPoint;

        // Ensure character entity is placed at the respawn point
        CharacterEntity.PositionAtSpawnPoint(spawnPoint);
        CharacterEntity.SetSpawnPose();

        CurrentZone = z;
        CurrentOutpostId = outpostId;

        var msg = new EnterZone
        {
            InstanceId = AssignedShard.InstanceId,
            ZoneId = CurrentZone.ID,
            ZoneTimestamp = CurrentZone.Timestamp,
            PreviewModeFlag = 0,
            ZoneOwner = "r5_exec",
            StreamingProtocol = 0x4c5f,
            Unk1_2 = 0x0c9f5,
            HotfixLevel = 0,
            MatchId = 0,
            Unk2 = 0,
            Unk3_Millis = 0x63e2db5e,
            ZoneName = CurrentZone.Name,
            HaveDevZoneInfo = 0,
            ZoneTimeSyncInfo = new ZoneTimeSyncData { FictionDateTimeOffsetMicros = 0, DayLengthFactor = 12.0F, DayPhaseOffset = 0.896445870399F },
            GameClockInfo = new GameClockInfoData
            {
                MicroUnix_1 = 1478970208392232,
                MicroUnix_2 = 1478774752697322,
                Timescale = 1.0,
                Unk3 = 0,
                Unk4 = 0,
                Paused = 0
            },
            SpectatorModeFlag = 0
        };

        NetChannels[ChannelType.Matrix].SendIAero(msg);

        Status = IPlayer.PlayerStatus.Loading;
        _lastKeyFrame = AssignedShard.CurrentTime;
    }
}