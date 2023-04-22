using GameServer.Data;
using GameServer.Test;
using System.Net;
using System.Numerics;
using System.Threading;
using Character = GameServer.Entities.Character.Character;
using AeroMessages.GSS.V66;
using AeroMessages.Matrix.V25;
using EnterZone = AeroMessages.Matrix.V25.EnterZone;
using WelcomeToTheMatrix = AeroMessages.Matrix.V25.WelcomeToTheMatrix;
using ForcedMovement = AeroMessages.GSS.V66.Character.Event.ForcedMovement;
using Respawned = AeroMessages.GSS.V66.Character.Event.Respawned;


namespace GameServer;

public class NetworkPlayer : NetworkClient, INetworkPlayer
{
    private double lastKeyFrame;

    public NetworkPlayer(IPEndPoint endPoint, uint socketId)
        : base(endPoint, socketId)
    {
        CharacterEntity = null;
        Status = IPlayer.PlayerStatus.Connecting;
    }

    public ulong PlayerId { get; protected set; }
    public ulong CharacterId { get; protected set; }
    public Character CharacterEntity { get; protected set; }
    public IPlayer.PlayerStatus Status { get; protected set; }
    public Zone CurrentZone { get; protected set; }
    public uint LastRequestedUpdate { get; set; }
    public uint RequestedClientTime { get; set; }
    public bool FirstUpdateRequested { get; set; }
    public ulong SteamUserId { get; set; }

    public void Init(IShard shard)
    {
        Init(this, shard, shard);
    }

    public void Login(ulong characterId)
    {
        PlayerId = 0x4658281c142e9f00ul;
        CharacterId = characterId;
        CharacterEntity = new Character(AssignedShard, characterId & 0xffffffffffffff00);
        CharacterEntity.Load(characterId);
        Status = IPlayer.PlayerStatus.LoggedIn;

        // WelcomeToTheMatrix
        var wel = new WelcomeToTheMatrix
        {
            PlayerID = PlayerId,
            Unk1 = new byte[0],
            Unk2 = new byte[0]
        };
        NetChannels[ChannelType.Matrix].SendIAero(wel);

        var zone = (uint)(characterId & 0x000000000000ffff);

        Program.Logger.Verbose("Zone {0}", zone);

        EnterZone(DataUtils.GetZone(zone));
    }

    public void Respawn()
    {
        var pointOfInterestPosition = DataUtils.GetZone(CurrentZone.ID).POIs["spawn"];
        CharacterEntity.Position = pointOfInterestPosition;

        var forcedMove = new ForcedMovement
        {
            Data = new ForcedMovementData
            {
                Type = 1,
                Unk1 = 0,
                HaveUnk2 = 0,
                Params1 = new ForcedMovementType1Params
                {
                    Position = p,
                    Direction = CharacterEntity.AimDirection,
                    Velocity = Vector3.Zero,
                    Time = AssignedShard.CurrentTime,
                }
            },
            ShortTime = AssignedShard.CurrentShortTime
        };
        NetChannels[ChannelType.ReliableGss].SendIAero(forcedMove, CharacterEntity.EntityId);

        var respawnMsg = new AeroMessages.GSS.V66.Character.Event.Respawned
        {
            ShortTime = AssignedShard.CurrentShortTime,
            Unk1 = 0,
            Unk2 = 0,
        };
        NetChannels[ChannelType.ReliableGss].SendIAero(respawnMsg, CharacterEntity.EntityId);

        var baseController = new AeroMessages.GSS.V66.Character.Controller.BaseController
        {
            CharacterStateProp = new AeroMessages.GSS.V66.Character.CharacterStateData
            {
                State = AeroMessages.GSS.V66.Character.CharacterStateData.CharacterStatus.Living,
                Time = AssignedShard.CurrentTime
            },
            CurrentHealthProp = CharacterEntity.CharData.MaxHealth,
            MaxHealthProp = new AeroMessages.GSS.V66.Character.MaxVital
            {
                Value = CharacterEntity.CharData.MaxHealth,
                Time = AssignedShard.CurrentTime
            },
            CurrentShieldsProp = 0,
            RegionUnlocksProp = 0xFFFFFFFFFFFFFFFFUL,
            GibVisualsIdProp = new AeroMessages.GSS.V66.Character.GibVisuals
            {
                Id = 0,
                Time = AssignedShard.CurrentTime
            },
            TimedDailyRewardProp = new AeroMessages.GSS.V66.Character.Controller.TimedDailyRewardData
            {
                Unk2 = 1,
                Unk4 = 1,
                Unk5 = AssignedShard.CurrentTime
            },
            PersonalFactionStanceProp = new PersonalFactionStanceData
            {
                Unk1 = new PersonalFactionStanceBitfield
                {
                    NumFactions = 50,
                    Bitfield = new byte[8] { 0x09, 0x0e, 0x5d, 0xff, 0x5f, 0x08, 0x00, 0x00 }
                },
                Unk2 = new PersonalFactionStanceBitfield
                {
                    NumFactions = 50,
                    Bitfield = new byte[8] { 0xf2, 0x00, 0x20, 0x00, 0x00, 0xf2, 0x00, 0x00 }
                }
            },
            RespawnTimesProp = null,



        };
        NetChannels[ChannelType.ReliableGss].SendIAeroChanges(baseController, CharacterEntity.EntityId);

        var combatController = new AeroMessages.GSS.V66.Character.Controller.CombatController
        {
            CombatTimer_0Prop = AssignedShard.CurrentTime,

            StatusEffectsChangeTime_10Prop = AssignedShard.CurrentShortTime,
            StatusEffects_10Prop = new StatusEffectData
            {
                Id = 986,
                Time = AssignedShard.CurrentTime,
                Initiator = new AeroMessages.Common.EntityId { Backing = Player.CharacterEntity.EntityId },
                MoreDataFlag = 0
            },

            StatusEffectsChangeTime_11Prop = AssignedShard.CurrentShortTime,
            StatusEffects_11Prop = new StatusEffectData
            {
                Id = 472,
                Time = AssignedShard.CurrentTime,
                Initiator = new AeroMessages.Common.EntityId { Backing = Player.CharacterEntity.EntityId },
                MoreDataFlag = 0
            }
        };
        NetChannels[ChannelType.ReliableGss].SendIAeroChanges(combatController, CharacterEntity.EntityId);


        // InventoryUpdate
        var inventoryUpdate = new AeroMessages.GSS.V66.Character.Event.InventoryUpdate
        {
            ClearExistingData = 1,
            ItemsPart1Length = 2,
            ItemsPart1 = new AeroMessages.GSS.V66.Character.Event.Item[2] {
                new AeroMessages.GSS.V66.Character.Event.Item {
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
                    Unk6 = new AeroMessages.GSS.V66.Character.Event.ItemUnkData[0] {},
                    Unk7 = 0,
                    Modules = new uint[0] {},
                },
                new AeroMessages.GSS.V66.Character.Event.Item {
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
                    Unk6 = new AeroMessages.GSS.V66.Character.Event.ItemUnkData[0] {},
                    Unk7 = 0,
                    Modules = new uint[0] {},
                }
            },
            ItemsPart2Length = 0,
            ItemsPart2 = new AeroMessages.GSS.V66.Character.Event.Item[0]{},
            ItemsPart3Length = 0,
            ItemsPart3 = new AeroMessages.GSS.V66.Character.Event.Item[0]{},
            Resources = new AeroMessages.GSS.V66.Character.Event.Resource[0]{},
            Loadouts = new AeroMessages.GSS.V66.Character.Event.Loadout[] {
                new AeroMessages.GSS.V66.Character.Event.Loadout {
                    FrameLoadoutId = 184538131,
                    Unk = 1535539622,
                    LoadoutName = "ODM \"Mammoth\"",
                    LoadoutType = "battleframe",
                    ChassisID = 76331,
                    LoadoutConfigs = new AeroMessages.GSS.V66.Character.Event.LoadoutConfig[2] {
                        new AeroMessages.GSS.V66.Character.Event.LoadoutConfig {
                            ConfigID = 0,
                            ConfigName = "pve",
                            Items = new AeroMessages.GSS.V66.Character.Event.LoadoutConfig_Item[0] {},
                            Visuals = new AeroMessages.GSS.V66.Character.LoadoutConfig_Visual[5] {
                                new AeroMessages.GSS.V66.Character.LoadoutConfig_Visual {
                                    ItemSdbId = 10000,
                                    VisualType = AeroMessages.GSS.V66.Character.LoadoutConfig_Visual.LoadoutVisualType.Decal,
                                    Data1 = 0,
                                    Data2 = 4294967295,
                                    Transform = new float[12] {
                                        0.052F,
                                        0.020F,
                                        0.000F,
                                        0.007F,
                                        -0.020F,
                                        -0.052F,
                                        0.018F,
                                        -0.048F,
                                        0.021F,
                                        0.108F,
                                        -0.105F,
                                        1.495F
                                    }
                                },
                                new AeroMessages.GSS.V66.Character.LoadoutConfig_Visual {
                                    ItemSdbId = 81423,
                                    VisualType = AeroMessages.GSS.V66.Character.LoadoutConfig_Visual.LoadoutVisualType.Glider,
                                    Data1 = 0,
                                    Data2 = 0,
                                    Transform = new float[0] {}
                                },
                                new AeroMessages.GSS.V66.Character.LoadoutConfig_Visual {
                                    ItemSdbId = 77087,
                                    VisualType = AeroMessages.GSS.V66.Character.LoadoutConfig_Visual.LoadoutVisualType.Vehicle,
                                    Data1 = 0,
                                    Data2 = 0,
                                    Transform = new float[0] {}
                                },
                                new AeroMessages.GSS.V66.Character.LoadoutConfig_Visual {
                                    ItemSdbId = 85163,
                                    VisualType = AeroMessages.GSS.V66.Character.LoadoutConfig_Visual.LoadoutVisualType.Palette,
                                    Data1 = 0,
                                    Data2 = 0,
                                    Transform = new float[0] {}
                                },
                                new AeroMessages.GSS.V66.Character.LoadoutConfig_Visual {
                                    ItemSdbId = 10022,
                                    VisualType = AeroMessages.GSS.V66.Character.LoadoutConfig_Visual.LoadoutVisualType.Pattern,
                                    Data1 = 1,
                                    Data2 = 0,
                                    Transform = new float[4] {
                                        0.000F,
                                        13572.00F,
                                        0.000F,
                                        1963.000F
                                    }
                                }
                            },
                            Perks = new uint[10] {
                                85817,
                                85818,
                                85956,
                                85976,
                                86067,
                                86137,
                                86139,
                                118819,
                                124247,
                                140713
                            },
                            Unk1 = 1464475061, // Did I... mess up the bandwidth? :thinking:
                            PerkBandwidth = 0,
                            PerkRespecLockRemainingSeconds = 0,
                            HaveExtraData = 0
                        },
                        new AeroMessages.GSS.V66.Character.Event.LoadoutConfig {
                            ConfigID = 1,
                            ConfigName = "pvp",
                            Items = new AeroMessages.GSS.V66.Character.Event.LoadoutConfig_Item[0] {},
                            Visuals = new AeroMessages.GSS.V66.Character.LoadoutConfig_Visual[0] {},
                            Perks = new uint[0],
                            Unk1 = 0,
                            PerkBandwidth = 0,
                            PerkRespecLockRemainingSeconds = 0,
                            HaveExtraData = 0
                        }
                    },
                    
                }
            },
            Unk = 1,
            SecondItems = new AeroMessages.GSS.V66.Character.Event.Item[0]{},
            SecondResources = new AeroMessages.GSS.V66.Character.Event.Resource[0]{}
        };
        NetChannels[ChannelType.ReliableGss].SendIAero(inventoryUpdate, CharacterEntity.EntityId);

    }

    public void Ready()
    {
        Status = IPlayer.PlayerStatus.Playing;
        CharacterEntity.Alive = true;
    }

    public void Jump()
    {
        //NetChannels[ChannelType.UnreliableGss].SendGSSClass( new JumpActioned { JumpTime = AssignedShard.CurrentShortTime }, CharacterEntity.EntityID, Enums.GSS.Controllers.Character_BaseController );
        CharacterEntity.TimeSinceLastJump = AssignedShard.CurrentShortTime;
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        // TODO: Implement FSM here to move player thru log in process to connecting to a shard to playing
        if (Status == IPlayer.PlayerStatus.Connected)
        {
            Status = IPlayer.PlayerStatus.LoggingIn;
        }
        else if (Status == IPlayer.PlayerStatus.Loading)
        {
        }
        else if (Status == IPlayer.PlayerStatus.Playing)
        {
            if (AssignedShard.CurrentTime - lastKeyFrame > 0.5)
            {
                //NetChannels[ChannelType.ReliableGss].SendGSSClass(Test.GSS.Character.BaseController.KeyFrame.Test(this, this), this.InstanceID, msgEnumType: typeof(Enums.GSS.Character.Events));
            }
        }
    }

    public void EnterZone(Zone z)
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
            Unk1_1 = 0x4c5f,
            Unk1_2 = 0x0c9f5,
            HotfixLevel = 0,
            MatchId = 0,
            Unk2 = 0,
            Unk3_Millis = 0x63e2db5e,
            ZoneName = CurrentZone.Name,
            HaveDevZoneInfo = 0,
            ZoneTimeSyncInfo = new ZoneTimeSyncData
            {
                FictionDateTimeOffsetMicros = 0,
                DayLengthFactor = 12.0F,
                DayPhaseOffset = 0.896445870399F,
            },
            GameClockInfo = new GameClockInfoData
            {
                MicroUnix_1 = 1478970208392232,
                MicroUnix_2 = 1478774752697322,
                Timescale = 1.0,
                Unk3 = 0,
                Unk4 = 0,
                Paused = 0,
            },
            SpectatorModeFlag = 0,
        };

        NetChannels[ChannelType.Matrix].SendIAero(msg);

        Status = IPlayer.PlayerStatus.Loading;
        lastKeyFrame = AssignedShard.CurrentTime;
    }
}