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
using GameServer.Test;
using Grpc.Net.Client;
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
    }

    public ulong PlayerId { get; private set; }
    public ulong CharacterId { get; private set; }
    public CharacterEntity CharacterEntity { get; private set; }
    public IPlayer.PlayerStatus Status { get; private set; }
    public Zone CurrentZone { get; private set; }
    public uint LastRequestedUpdate { get; set; }
    public uint RequestedClientTime { get; set; }
    public bool FirstUpdateRequested { get; set; }
    public ulong SteamUserId { get; set; }

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

        CharacterEntity = new CharacterEntity(AssignedShard, guid);

        CharacterAndBattleframeVisuals remoteData = null;
        try
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5201");
            var client = new GameServerAPI.GameServerAPIClient(channel);
            remoteData = await client.GetCharacterAndBattleframeVisualsAsync(new CharacterID { ID = (long)characterId });
        }
        catch
        {
            Console.WriteLine($"Could not get character over GRPC, will use fallback");
        }

        bool useRemoteData = true;
        if (remoteData != null && useRemoteData)
        {
            CharacterEntity.LoadRemote(remoteData);
        }
        else
        {
            CharacterEntity.Load(HardcodedCharacterData.FallbackData);
        }

        CharacterEntity.SetControllingPlayer(this);
        CharacterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Spawning, AssignedShard.CurrentTime);
        Status = IPlayer.PlayerStatus.LoggedIn;

        // WelcomeToTheMatrix
        var wel = new WelcomeToTheMatrix { PlayerID = PlayerId, Unk1 = Array.Empty<byte>(), Unk2 = Array.Empty<byte>() };
        NetChannels[ChannelType.Matrix].SendIAero(wel);

        var zone = (uint)(characterId & 0x000000000000ffff);

        Logger.Verbose("Zone {0}", zone);

        // Ensure character entity is placed at the respawn point
        var pointOfInterestPosition = DataUtils.GetZone(zone).POIs["spawn"];
        CharacterEntity.SetPosition(pointOfInterestPosition);
        CharacterEntity.SetSpawnPose();

        EnterZone(DataUtils.GetZone(zone));
    }

    public void EnterZoneAck()
    {
        AssignedShard.EntityMan.Add(CharacterEntity.EntityId, CharacterEntity);
        AssignedShard.EntityMan.ScopeIn(this, CharacterEntity);
    }

    public void Respawn()
    {
        var pointOfInterestPosition = DataUtils.GetZone(CurrentZone.ID).POIs["spawn"];
        CharacterEntity.SetPosition(pointOfInterestPosition);
        CharacterEntity.SetSpawnTime(AssignedShard.CurrentTime);
        var forcedMove = new ForcedMovement
        {
            Data = new ForcedMovementData
            {
                Type = 1,
                Unk1 = 0,
                HaveUnk2 = 0,
                Params1 = new ForcedMovementType1Params { Position = pointOfInterestPosition, Direction = CharacterEntity.AimDirection, Velocity = Vector3.Zero, Time = AssignedShard.CurrentTime + 1 }
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
        // What are you doing here?
        var inventoryUpdate = new InventoryUpdate
        {
            ClearExistingData = 1,
            ItemsPart1Length = 4,
            ItemsPart1 = new Item[]
            {
                new()
                {
                    Unk1 = 0,
                    SdbId = 76331,
                    GUID = 744961712419132925ul,
                    SubInventory = 4,
                    Unk2 = 0x22BEA256,
                    DynamicFlags = 0,
                    Durability = 0,
                    Unk3 = 0,
                    Unk4 = 0,
                    Unk5 = 1,
                    Unk6 = Array.Empty<ItemUnkData>(),
                    Unk7 = 0,
                    Modules = Array.Empty<uint>()
                },
                new()
                {
                    Unk1 = 0,
                    SdbId = 76331,
                    GUID = 9181641073530142461ul,
                    SubInventory = 4,
                    Unk2 = 0x964C1352,
                    DynamicFlags = 0,
                    Durability = 0,
                    Unk3 = 0,
                    Unk4 = 0,
                    Unk5 = 1,
                    Unk6 = Array.Empty<ItemUnkData>(),
                    Unk7 = 0,
                    Modules = Array.Empty<uint>()
                },
                new()
                {
                    Unk1 = 0,
                    SdbId = 81423, // Glider
                    GUID = 9203082006052811773ul,
                    SubInventory = 2,
                    Unk2 = 0x518973AE,
                    DynamicFlags = 0,
                    Durability = 0,
                    Unk3 = 0,
                    Unk4 = 0,
                    Unk5 = 1,
                    Unk6 = Array.Empty<ItemUnkData>(),
                    Unk7 = 0,
                    Modules = Array.Empty<uint>()
                },
                new()
                {
                    Unk1 = 0,
                    SdbId = 77087, // LGV
                    GUID = 9168405683759816701ul,
                    SubInventory = 2,
                    Unk2 = 0x582A0A04,
                    DynamicFlags = 0,
                    Durability = 0,
                    Unk3 = 0,
                    Unk4 = 0,
                    Unk5 = 1,
                    Unk6 = Array.Empty<ItemUnkData>(),
                    Unk7 = 0,
                    Modules = Array.Empty<uint>()
                }
            },
            ItemsPart2Length = 0,
            ItemsPart2 = Array.Empty<Item>(),
            ItemsPart3Length = 0,
            ItemsPart3 = Array.Empty<Item>(),
            Resources = new Resource[]
            {
                new()
                {
                    SdbId = 10, // Crystite
                    Quantity = 10000,
                    SubInventory = 1,
                    TextKey = string.Empty,
                    Unk2 = 0,
                },
                new()
                {
                    SdbId = 30101, // Credits
                    Quantity = 10000,
                    SubInventory = 1,
                    TextKey = string.Empty,
                    Unk2 = 0,
                }
            },
            Loadouts = HardcodedCharacterData.GetTempAvailableLoadouts(),
            /*
            new Loadout[]
            {
                new()
                {
                    FrameLoadoutId = 184538131,
                    Unk = 1535539622,
                    LoadoutName = "ODM \"Mammoth\"",
                    LoadoutType = "battleframe",
                    ChassisID = 76331,
                    LoadoutConfigs = new LoadoutConfig[]
                    {
                        new()
                        {
                            ConfigID = 0,
                            ConfigName = "pve",
                            Items = Array.Empty<LoadoutConfig_Item>(),
                            Visuals = new LoadoutConfig_Visual[]
                            {
                                new()
                                {
                                    ItemSdbId = 10000,
                                    VisualType = LoadoutConfig_Visual.LoadoutVisualType.Decal,
                                    Data1 = 0,
                                    Data2 = 4294967295,
                                    Transform = new[]
                                                {
                                                    0.052F, 0.020F, 0.000F, 0.007F, -0.020F, -0.052F, 0.018F, -0.048F, 0.021F, 0.108F, -0.105F, 1.495F
                                                }
                                },
                                new()
                                {
                                    ItemSdbId = 81423,
                                    VisualType = LoadoutConfig_Visual.LoadoutVisualType.Glider,
                                    Data1 = 0,
                                    Data2 = 0,
                                    Transform = Array.Empty<float>()
                                },
                                new()
                                {
                                    ItemSdbId = 77087,
                                    VisualType = LoadoutConfig_Visual.LoadoutVisualType.Vehicle,
                                    Data1 = 0,
                                    Data2 = 0,
                                    Transform = Array.Empty<float>()
                                },
                                new()
                                {
                                    ItemSdbId = 85163,
                                    VisualType = LoadoutConfig_Visual.LoadoutVisualType.Palette,
                                    Data1 = 0,
                                    Data2 = 0,
                                    Transform = Array.Empty<float>()
                                },
                                new()
                                {
                                    ItemSdbId = 10022,
                                    VisualType = LoadoutConfig_Visual.LoadoutVisualType.Pattern,
                                    Data1 = 1,
                                    Data2 = 0,
                                    Transform = new[] { 0.000F, 13572.00F, 0.000F, 1963.000F }
                                }
                            },
                            Perks = new uint[] { 85817, 85818, 85956, 85976, 86067, 86137, 86139, 118819, 124247, 140713 },
                            Unk1 = 1464475061, // Did I... mess up the bandwidth? :thinking:
                            PerkBandwidth = 0,
                            PerkRespecLockRemainingSeconds = 0,
                            HaveExtraData = 0
                        },
                        new()
                        {
                            ConfigID = 1,
                            ConfigName = "pvp",
                            Items = Array.Empty<LoadoutConfig_Item>(),
                            Visuals = Array.Empty<LoadoutConfig_Visual>(),
                            Perks = Array.Empty<uint>(),
                            Unk1 = 0,
                            PerkBandwidth = 0,
                            PerkRespecLockRemainingSeconds = 0,
                            HaveExtraData = 0
                        }
                    }
                }
            },
            */
            Unk = 1,
            SecondItems = Array.Empty<Item>(),
            SecondResources = Array.Empty<Resource>()
        };
        NetChannels[ChannelType.ReliableGss].SendIAero(inventoryUpdate, CharacterEntity.EntityId);

        AssignedShard.EntityMan.ScopeInAll(this); // Move elsewhere when adding actual respawning as we only need to call this once
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

    private void EnterZone(Zone z)
    {
        CurrentZone = z;
        CharacterEntity.Position = CurrentZone.POIs["spawn"];

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