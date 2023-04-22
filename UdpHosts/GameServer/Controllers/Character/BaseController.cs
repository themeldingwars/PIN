using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Controller;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Character.View;
using GameServer.Entities.Character;
using GameServer.Enums.GSS.Character;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
public class BaseController : Base
{
    private ILogger _logger;

    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        _logger = logger;

        var cd = player.CharacterEntity.CharData;

        var staticInfo = new StaticInfoData
                         {
                             DisplayName = cd.Name,
                             UniqueName = cd.Name,
                             Gender = (byte)cd.Gender,
                             Race = (byte)cd.Race,
                             CharInfoId = cd.CharInfoID,
                             HeadMain = cd.CharVisuals.HeadMain,
                             Eyes = cd.CharVisuals.Eyes,
                             Unk_1 = 0xff,
                             IsNPC = 0,
                             StaffFlags = 0x3,
                             CharacterTypeId = cd.CharVisuals.CharTypeID,
                             VoiceSet = cd.VoiceSet,
                             TitleId = cd.TitleID,
                             NameLocalizationId = cd.NameLocalizationID,
                             HeadAccessories = ((List<uint>)cd.CharVisuals.HeadAccessories).ToArray(),
                             LoadoutVehicle = cd.Loadout.VehicleID,
                             LoadoutGlider = cd.Loadout.GliderID,
                             Visuals = new VisualsBlock
                                       {
                                           Decals = Array.Empty<VisualsDecalsBlock>(),
                                           Gradients = Array.Empty<uint>(),
                                           Colors = ((List<uint>)cd.CharVisuals.Colors).ToArray(),
                                           Palettes = Array.Empty<VisualsPaletteBlock>(),
                                           Patterns = Array.Empty<VisualsPatternBlock>(),
                                           OrnamentGroupIds = ((List<uint>)cd.CharVisuals.OrnamentGroups).ToArray(),
                                           CziMapAssetIds = Array.Empty<uint>(),
                                           MorphWeights = Array.Empty<HalfFloat>(),
                                           Overlays = Array.Empty<VisualsOverlayBlock>()
                                       },
                             ArmyTag = cd.Army.Name
                         };

        var gibVisuals = new GibVisuals { Id = 0, Time = shard.CurrentTime };
        var processDelay = new ProcessDelayData { Unk1 = 30721, Unk2 = 236 };
        var characterState = new CharacterStateData { State = CharacterStateData.CharacterStatus.Respawning, Time = shard.CurrentTime };
        var hostilityInfo = new HostilityInfoData { Flags = 0 | HostilityInfoData.HostilityFlags.Faction, FactionId = 1 };
        var maxShields = new MaxVital { Value = 0, Time = shard.CurrentTime };
        var maxHealth = new MaxVital { Value = 19192, Time = shard.CurrentTime };
        var emote = new EmoteData { Id = 0, Time = 0 };
        var dockedParams = new DockedParamsData { Unk1 = new EntityId { Backing = 0 }, Unk2 = Vector3.Zero, Unk3 = 0 };
        var assetOverrides = new AssetOverridesField { Ids = Array.Empty<uint>() };

        var currentEquipment = new EquipmentData
                               {
                                   Chassis = new SlottedItem
                                             {
                                                 SdbId = cd.Loadout.ChassisID,
                                                 SlotIndex = 0,
                                                 Flags = 0,
                                                 Unk2 = 0,
                                                 Modules = Array.Empty<SlottedModule>(),
                                                 Visuals = new VisualsBlock
                                                           {
                                                               Decals = new VisualsDecalsBlock[]
                                                                        {
                                                                            new()
                                                                            {
                                                                                DecalId = 10000,
                                                                                Color = 4294967295,
                                                                                Usage = 255,
                                                                                Transform = new HalfVector4[]
                                                                                            {
                                                                                                new()
                                                                                                {
                                                                                                    X = new HalfFloat { Value = 10935 },
                                                                                                    Y = new HalfFloat { Value = 9478 },
                                                                                                    Z = new HalfFloat { Value = 0 },
                                                                                                    W = new HalfFloat { Value = 8106 }
                                                                                                },
                                                                                                new()
                                                                                                {
                                                                                                    X = new HalfFloat { Value = 42272 },
                                                                                                    Y = new HalfFloat { Value = 43680 },
                                                                                                    Z = new HalfFloat { Value = 9380 },
                                                                                                    W = new HalfFloat { Value = 43573 }
                                                                                                },
                                                                                                new()
                                                                                                {
                                                                                                    X = new HalfFloat { Value = 9592 },
                                                                                                    Y = new HalfFloat { Value = 12012 },
                                                                                                    Z = new HalfFloat { Value = 44736 },
                                                                                                    W = new HalfFloat { Value = 15867 }
                                                                                                }
                                                                                            }
                                                                            }
                                                                        },
                                                               Gradients = Array.Empty<uint>(),
                                                               Colors = ((List<uint>)cd.ChassisVisuals.Colors).ToArray(),
                                                               Palettes = new VisualsPaletteBlock[] { new() { PaletteId = 85163, PaletteType = 0 } },
                                                               Patterns = new VisualsPatternBlock[]
                                                                          {
                                                                              new()
                                                                              {
                                                                                  PatternId = 10022,
                                                                                  TransformValues = new HalfVector4
                                                                                                    {
                                                                                                        X = new HalfFloat { Value = 0 },
                                                                                                        Y = new HalfFloat { Value = 16384 },
                                                                                                        Z = new HalfFloat { Value = 0 },
                                                                                                        W = new HalfFloat { Value = 0 }
                                                                                                    }
                                                                              }
                                                                          },
                                                               OrnamentGroupIds = Array.Empty<uint>(),
                                                               CziMapAssetIds = Array.Empty<uint>(),
                                                               MorphWeights = Array.Empty<HalfFloat>(),
                                                               Overlays = Array.Empty<VisualsOverlayBlock>()
                                                           }
                                             },
                                   Backpack = new SlottedItem
                                              {
                                                  SdbId = cd.Loadout.BackpackID,
                                                  SlotIndex = 0,
                                                  Flags = 0,
                                                  Unk2 = 0,
                                                  Modules = Array.Empty<SlottedModule>(),
                                                  Visuals = new VisualsBlock
                                                            {
                                                                Decals = Array.Empty<VisualsDecalsBlock>(),
                                                                Gradients = Array.Empty<uint>(),
                                                                Colors = Array.Empty<uint>(),
                                                                Palettes = Array.Empty<VisualsPaletteBlock>(),
                                                                Patterns = Array.Empty<VisualsPatternBlock>(),
                                                                OrnamentGroupIds = Array.Empty<uint>(),
                                                                CziMapAssetIds = Array.Empty<uint>(),
                                                                MorphWeights = Array.Empty<HalfFloat>(),
                                                                Overlays = Array.Empty<VisualsOverlayBlock>()
                                                            }
                                              },
                                   PrimaryWeapon = new SlottedWeapon
                                                   {
                                                       Item = new SlottedItem
                                                              {
                                                                  SdbId = cd.Loadout.PrimaryWeaponID,
                                                                  SlotIndex = 0,
                                                                  Flags = 0,
                                                                  Unk2 = 0,
                                                                  Modules = Array.Empty<SlottedModule>(),
                                                                  Visuals = new VisualsBlock
                                                                            {
                                                                                Decals = Array.Empty<VisualsDecalsBlock>(),
                                                                                Gradients = Array.Empty<uint>(),
                                                                                Colors = new uint[] { 0x322c0000, 0x543110a2, 0x65b42104 },
                                                                                Palettes = Array.Empty<VisualsPaletteBlock>(),
                                                                                Patterns = Array.Empty<VisualsPatternBlock>(),
                                                                                OrnamentGroupIds = Array.Empty<uint>(),
                                                                                CziMapAssetIds = Array.Empty<uint>(),
                                                                                MorphWeights = Array.Empty<HalfFloat>(),
                                                                                Overlays = Array.Empty<VisualsOverlayBlock>()
                                                                            }
                                                              },
                                                       Unk1 = 0,
                                                       Unk2 = 0
                                                   },
                                   SecondaryWeapon = new SlottedWeapon
                                                     {
                                                         Item = new SlottedItem
                                                                {
                                                                    SdbId = cd.Loadout.SecondaryWeaponID,
                                                                    SlotIndex = 0,
                                                                    Flags = 0,
                                                                    Unk2 = 0,
                                                                    Modules = Array.Empty<SlottedModule>(),
                                                                    Visuals = new VisualsBlock
                                                                              {
                                                                                  Decals = Array.Empty<VisualsDecalsBlock>(),
                                                                                  Gradients = Array.Empty<uint>(),
                                                                                  Colors = new uint[] { 0x322c0000, 0x543110a2, 0x65b42104 },
                                                                                  Palettes = Array.Empty<VisualsPaletteBlock>(),
                                                                                  Patterns = Array.Empty<VisualsPatternBlock>(),
                                                                                  OrnamentGroupIds = Array.Empty<uint>(),
                                                                                  CziMapAssetIds = Array.Empty<uint>(),
                                                                                  MorphWeights = Array.Empty<HalfFloat>(),
                                                                                  Overlays = Array.Empty<VisualsOverlayBlock>()
                                                                              }
                                                                },
                                                         Unk1 = 0,
                                                         Unk2 = 0
                                                     },
                                   EndUnk1 = 0,
                                   EndUnk2 = 0
                               };

        var spawnPose = new CharacterSpawnPose
                        {
                            Time = shard.CurrentTime,
                            Position = player.CharacterEntity.Position,
                            Rotation = player.CharacterEntity.Rotation,
                            AimDirection = player.CharacterEntity.AimDirection,
                            Velocity = player.CharacterEntity.Velocity,
                            MovementState = 0x1000,
                            Unk1 = 0,
                            Unk2 = 0,
                            JetpackEnergy = 0x639c,
                            AirGroundTimer = 0,
                            JumpTimer = 0,
                            HaveDebugData = 0
                        };

        var energyParams = new EnergyParamsData { Max = 1000.0f, Delay = 500, Recharge = 156.0f, Time = shard.CurrentTime };

        var characterStats = new CharacterStatsData
                             {
                                 ItemAttributes = new StatsData[]
                                                  {
                                                      new() { Id = 5, Value = 156.414169f }, new() { Id = 6, Value = 1037.8347f }, new() { Id = 7, Value = 177.44128f }, new() { Id = 12, Value = 16.250000f }, new() { Id = 35, Value = 300 },
                                                      new() { Id = 36, Value = 250 }, new() { Id = 37, Value = 2.092090f }, new() { Id = 142, Value = 12.55f }, new() { Id = 143, Value = 1136 }, new() { Id = 144, Value = 18.433180f },
                                                      new() { Id = 173, Value = 10 }, new() { Id = 186, Value = 11.40f }, new() { Id = 959, Value = 1 }, new() { Id = 1050, Value = 34.5f }, new() { Id = 1051, Value = 13.824884f },
                                                      new() { Id = 1052, Value = 5.5f }, new() { Id = 1121, Value = 150 }, new() { Id = 1146, Value = 10.0f }, new() { Id = 1367, Value = 85 }, new() { Id = 1368, Value = 100 },
                                                      new() { Id = 1370, Value = 65 }, new() { Id = 1371, Value = 120 }, new() { Id = 1372, Value = 140 }, new() { Id = 1377, Value = 140.531250f }, new() { Id = 1395, Value = 75 },
                                                      new() { Id = 1419, Value = 32.769249f }, new() { Id = 1420, Value = 16901.744141f }, new() { Id = 1439, Value = 15279.667969f }, new() { Id = 1451, Value = 681 },
                                                      new() { Id = 1583, Value = 1 }, new() { Id = 1620, Value = 5049.767090f }, new() { Id = 1622, Value = 8 }, new() { Id = 1733, Value = 1.800000f }, new() { Id = 1736, Value = 60 },
                                                      new() { Id = 1737, Value = 5486.919434f }, new() { Id = 1746, Value = 9.320923f }, new() { Id = 1785, Value = 1.084000f }, new() { Id = 1835, Value = 5932.512207f },
                                                      new() { Id = 1904, Value = 4 }, new() { Id = 1905, Value = 2 }, new() { Id = 1987, Value = 8 }, new() { Id = 2034, Value = 22 }, new() { Id = 2037, Value = 9887.518555f },
                                                      new() { Id = 2039, Value = 9 }, new() { Id = 2042, Value = 12.252850f }
                                                  },
                                 Unk1 = 0,
                                 WeaponA = Array.Empty<StatsData>(),
                                 Unk2 = 0,
                                 WeaponB = Array.Empty<StatsData>(),
                                 Unk3 = 0,
                                 AttributeCategories1 = Array.Empty<StatsData>(),
                                 AttributeCategories2 = Array.Empty<StatsData>()
                             };

        var scopeBubble = new ScopeBubbleInfoData { Unk1 = 0, Unk2 = 0 };

        var visualOverrides = new VisualOverridesField { Data = Array.Empty<VisualOverridesData>() };

        var baseController = new AeroMessages.GSS.V66.Character.Controller.BaseController
                             {
                                 TimePlayedProp = 0,
                                 CurrentWeightProp = 0,
                                 EncumberedWeightProp = 255,
                                 AuthorizedTerminalProp = new AuthorizedTerminalData { TerminalType = 0, TerminalId = 0, TerminalEntityId = 0 },
                                 PingTimeProp = 0, // shard.CurrentTime,
                                 StaticInfoProp = staticInfo,
                                 SpawnTimeProp = shard.CurrentTime,
                                 VisualOverridesProp = visualOverrides,
                                 CurrentEquipmentProp = currentEquipment,
                                 SelectedLoadoutProp = 184538131, // cd.Loadout.ChassisID,
                                 SelectedLoadoutIsPvPProp = 0,
                                 GibVisualsIdProp = gibVisuals,
                                 SpawnPoseProp = spawnPose,
                                 ProcessDelayProp = processDelay,
                                 SpectatorModeProp = 0,
                                 CinematicCameraProp = null,
                                 CharacterStateProp = characterState,
                                 HostilityInfoProp = hostilityInfo,
                                 PersonalFactionStanceProp = null,
                                 CurrentHealthProp = 0,
                                 CurrentShieldsProp = 0,
                                 MaxShieldsProp = maxShields,
                                 MaxHealthProp = maxHealth,
                                 CurrentDurabilityPctProp = 100,
                                 EnergyParamsProp = energyParams,
                                 CharacterStatsProp = characterStats,
                                 EmoteIDProp = emote,
                                 AttachedToProp = null,
                                 SnapMountProp = 0,
                                 SinFlagsProp = 0,
                                 SinFlagsPrivateProp = 0,
                                 SinFactionsAcquiredByProp = null,
                                 SinTeamsAcquiredByProp = null,
                                 ArmyGUIDProp = cd.Army.GUID,
                                 ArmyIsOfficerProp = 0,
                                 EncounterPartyTupleProp = null,
                                 DockedParamsProp = dockedParams,
                                 LookAtTargetProp = null,
                                 ZoneUnlocksProp = 0,
                                 RegionUnlocksProp = 0,
                                 ChatPartyLeaderIdProp = new EntityId { Backing = 0 },
                                 ScopeBubbleInfoProp = scopeBubble,
                                 CarryableObjects_0Prop = null,
                                 CarryableObjects_1Prop = null,
                                 CarryableObjects_2Prop = null,
                                 CachedAssetsProp = null,
                                 RespawnTimesProp = null,
                                 ProgressionXpProp = 0,
                                 PermanentStatusEffectsProp = new PermanentStatusEffectsData { Effects = Array.Empty<PermanentStatusEffectsInnerData>() },
                                 XpBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 XpPermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 XpZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 XpVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 XpEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ResourceBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ResourcePermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ResourceZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ResourceVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ResourceEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 MoneyBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 MoneyPermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 MoneyZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 MoneyVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 MoneyEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ReputationBoostModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ReputationPermanentModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ReputationZoneModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ReputationVipModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 ReputationEventModifierProp = new StatModifierData { ModifierId = 0, StatValue = 0.0f },
                                 WalletProp = new WalletData { Beans = 999, Epoch = 1462889864 },
                                 LoyaltyProp = new LoyaltyData { Current = 0, Lifetime = 0, Tier = 0 },
                                 LevelProp = 0,
                                 EffectiveLevelProp = 0,
                                 LevelResetCountProp = 0,
                                 OldestDeployablesProp = new OldestDeployablesField { Data = Array.Empty<OldestDeployablesData>() },
                                 PerkRespecsProp = 0,
                                 ArcStatusProp = null,
                                 LeaveZoneTimeProp = null,
                                 ChatMuteStatusProp = 0,
                                 TimedDailyRewardProp = new TimedDailyRewardData
                                                        {
                                                            Unk1 = 0,
                                                            Unk2 = 0,
                                                            Unk3 = 0,
                                                            Unk4 = 0,
                                                            Unk5 = 0
                                                        },
                                 TimedDailyRewardResultProp = null,
                                 SinCardTypeProp = 0,
                                 SinCardFields_0Prop = null,
                                 SinCardFields_1Prop = null,
                                 SinCardFields_2Prop = null,
                                 SinCardFields_3Prop = null,
                                 SinCardFields_4Prop = null,
                                 SinCardFields_5Prop = null,
                                 SinCardFields_6Prop = null,
                                 SinCardFields_7Prop = null,
                                 SinCardFields_8Prop = null,
                                 SinCardFields_9Prop = null,
                                 SinCardFields_10Prop = null,
                                 SinCardFields_11Prop = null,
                                 SinCardFields_12Prop = null,
                                 SinCardFields_13Prop = null,
                                 SinCardFields_14Prop = null,
                                 SinCardFields_15Prop = null,
                                 SinCardFields_16Prop = null,
                                 SinCardFields_17Prop = null,
                                 SinCardFields_18Prop = null,
                                 SinCardFields_19Prop = null,
                                 SinCardFields_20Prop = null,
                                 SinCardFields_21Prop = null,
                                 SinCardFields_22Prop = null,
                                 AssetOverridesProp = assetOverrides,
                                 FriendCountProp = 0, // :'(
                                 CAISStatusProp = new CAISStatusData { State = CAISStatusData.CAISState.None, Elapsed = 0 },
                                 ScalingLevelProp = 0,
                                 PvPRankProp = 0,
                                 PvPRankPointsProp = 0,
                                 PvPTokensProp = 0,
                                 BountyPointsLastClaimedProp = 0,
                                 EliteLevelProp = 1
                             };

        var combatController = new AeroMessages.GSS.V66.Character.Controller.CombatController
                               {
                                   RunSpeedMultProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   FwdRunSpeedMultProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   JumpHeightMultProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   AirControlMultProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   ThrustStrengthMultProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   ThrustAirControlProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   FrictionProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   AmmoConsumptionProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   MaxTurnRateProp = new StatMultiplierData { Value = 0f, Time = shard.CurrentTime },
                                   TurnSpeedProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   TimeDilationProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   FireRateModifierProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   AccuracyModifierProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   GravityMultProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   AirResistanceMultProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   WeaponChargeupModProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   WeaponDamageDealtModProp = new StatMultiplierData { Value = 1.0f, Time = shard.CurrentTime },
                                   FireMode_0Prop = new FireModeData { Mode = 0, Time = shard.CurrentTime },
                                   FireMode_1Prop = new FireModeData { Mode = 0, Time = shard.CurrentTime },
                                   WeaponIndexProp = new WeaponIndexData { Time = shard.CurrentTime },
                                   WeaponFireBaseTimeProp = new WeaponFireBaseTimeData { ChangeTime = 0, Unk = 0 },
                                   WeaponAgilityModProp = 1.0f,
                                   CombatFlagsProp = new CombatFlagsData { Value = 0, Time = shard.CurrentTime },
                                   PermissionFlagsProp = new PermissionFlagsData
                                                         {
                                                             Time = shard.CurrentTime,
                                                             Value = 0ul
                                                                     | PermissionFlagsData.CharacterPermissionFlags.movement
                                                                     | PermissionFlagsData.CharacterPermissionFlags.sprint
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_3
                                                                     | PermissionFlagsData.CharacterPermissionFlags.jump
                                                                     | PermissionFlagsData.CharacterPermissionFlags.weapon
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_5
                                                                     | PermissionFlagsData.CharacterPermissionFlags.abilities
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_7
                                                                     | PermissionFlagsData.CharacterPermissionFlags.jetpack
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_13
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_14
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_15
                                                                     | PermissionFlagsData.CharacterPermissionFlags.crouch
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_21
                                                                     | PermissionFlagsData.CharacterPermissionFlags.calldown_abilities
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_23
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_24
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_25
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_26
                                                                     | PermissionFlagsData.CharacterPermissionFlags.self_revive
                                                                     //| PermissionFlagsData.CharacterPermissionFlags.respawn_input
                                                                     | PermissionFlagsData.CharacterPermissionFlags.battleframe_abilities
                                                                     | PermissionFlagsData.CharacterPermissionFlags.unk_31
                                                         },
                                   NemesesProp = new NemesesData { Values = Array.Empty<ulong>() },
                                   SuperChargeProp = new SuperChargeData { Value = 0, Op = 0 }
                               };
        var effectsController = new LocalEffectsController();
        var missionController = new AeroMessages.GSS.V66.Character.Controller.MissionAndMarkerController();

        // Temp
        var observer = new ObserverView
                       {
                           StaticInfoProp = staticInfo,
                           SpawnTimeProp = shard.CurrentTime,
                           EffectsFlagsProp = 0,
                           GibVisualsIDProp = gibVisuals,
                           ProcessDelayProp = processDelay,
                           CharacterStateProp = characterState,
                           HostilityInfoProp = hostilityInfo,
                           PersonalFactionStanceProp = null,
                           CurrentHealthPctProp = 100,
                           MaxHealthProp = maxHealth,
                           EmoteIDProp = emote,
                           AttachedToProp = null,
                           SnapMountProp = 0,
                           SinFlagsProp = 0,
                           SinFactionsAcquiredByProp = null,
                           SinTeamsAcquiredByProp = null,
                           ArmyGUIDProp = baseController.ArmyGUIDProp,
                           OwnerIdProp = 0,
                           NPCTypeProp = 0,
                           DockedParamsProp = dockedParams,
                           LookAtTargetProp = null,
                           WaterLevelAndDescProp = 0,
                           CarryableObjects_0Prop = null,
                           CarryableObjects_1Prop = null,
                           CarryableObjects_2Prop = null,
                           RespawnTimesProp = null,
                           SinCardTypeProp = 0,
                           SinCardFields_0Prop = null,
                           SinCardFields_1Prop = null,
                           SinCardFields_2Prop = null,
                           SinCardFields_3Prop = null,
                           SinCardFields_4Prop = null,
                           SinCardFields_5Prop = null,
                           SinCardFields_6Prop = null,
                           SinCardFields_7Prop = null,
                           SinCardFields_8Prop = null,
                           SinCardFields_9Prop = null,
                           SinCardFields_10Prop = null,
                           SinCardFields_11Prop = null,
                           SinCardFields_12Prop = null,
                           SinCardFields_13Prop = null,
                           SinCardFields_14Prop = null,
                           SinCardFields_15Prop = null,
                           SinCardFields_16Prop = null,
                           SinCardFields_17Prop = null,
                           SinCardFields_18Prop = null,
                           SinCardFields_19Prop = null,
                           SinCardFields_20Prop = null,
                           SinCardFields_21Prop = null,
                           SinCardFields_22Prop = null,
                           AssetOverridesProp = assetOverrides
                       };

        var combat = new CombatView
                     {
                         FireMode_0Prop = combatController.FireMode_0Prop,
                         FireMode_1Prop = combatController.FireMode_1Prop,
                         WeaponIndexProp = combatController.WeaponIndexProp,
                         WeaponAgilityModProp = combatController.WeaponAgilityModProp,
                         CombatFlagsProp = combatController.CombatFlagsProp,
                         MimicParentProp = new EntityId { Backing = 0 },
                         MimicOffsetProp = Vector3.Zero
                     };

        var equipment = new EquipmentView
                        {
                            VisualOverridesProp = visualOverrides,
                            CurrentEquipmentProp = currentEquipment,
                            LevelProp = baseController.CurrentDurabilityPctProp,
                            CurrentDurabilityPctProp = baseController.CurrentDurabilityPctProp,
                            CharacterStatsProp = characterStats,
                            ScalingLevelProp = baseController.ScalingLevelProp,
                            PvPRankProp = baseController.PvPRankProp,
                            EliteLevelProp = baseController.EliteLevelProp
                        };

        var movement = new MovementView
                       {
                           MovementProp = new MovementData
                                          {
                                              Position = spawnPose.Position,
                                              Rotation = spawnPose.Rotation,
                                              Aim = spawnPose.AimDirection,
                                              MovementState = spawnPose.MovementState,
                                              Time = spawnPose.Time
                                          }
                       };

        // Controllers
        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(baseController, player.EntityId, player.PlayerId);
        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(combatController, player.EntityId, player.PlayerId);
        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(effectsController, player.EntityId, player.PlayerId);
        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(missionController, player.EntityId, player.PlayerId);
        client.NetChannels[ChannelType.ReliableGss].SendIAero(new CharacterLoaded(), player.EntityId);

        // Views
        client.NetChannels[ChannelType.ReliableGss].SendIAero(observer, player.EntityId, 3);
        client.NetChannels[ChannelType.ReliableGss].SendIAero(equipment, player.EntityId, 3);
        client.NetChannels[ChannelType.ReliableGss].SendIAero(combat, player.EntityId, 3);
        client.NetChannels[ChannelType.ReliableGss].SendIAero(movement, player.EntityId, 3);
    }

    [MessageID((byte)Commands.FetchQueueInfo)]
    public void FetchQueueInfo(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.FetchQueueInfo
        LogMissingImplementation<BaseController>(nameof(FetchQueueInfo), entityId, packet, _logger);
    }

    [MessageID((byte)Commands.PlayerReady)]
    public void PlayerReady(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        player.Ready();
    }

    [MessageID((byte)Commands.MovementInput)]
    public void MovementInput(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: This currently only handles PosRotState inputs, logic needs to be added for the other MovementDataTypes

        if (packet.BytesRemaining < 64)
        {
            return;
        }

        var movementInput = packet.Unpack<MovementInput>();

        if (!player.CharacterEntity.Alive)
        {
            return; // can't move if you're dead (or at least shouldn't o.o")
        }

        var poseData = movementInput.PoseData;
        var posRotState = poseData.PosRotState;

        player.CharacterEntity.Position = posRotState.Pos;
        player.CharacterEntity.Rotation = posRotState.Rot;
        player.CharacterEntity.Velocity = poseData.Velocity;
        player.CharacterEntity.AimDirection = poseData.Aim;

        var movementStateValue = posRotState.MovementState;
        player.CharacterEntity.MovementStateContainer.MovementState = (CharMovementState)movementStateValue;

        if (player.CharacterEntity.MovementStateContainer.InvalidFlags != CharMovementState.None)
        {
            _logger.Error($"Unmapped {nameof(CharMovementState)} encountered! \n{player.CharacterEntity.MovementStateContainer}");
        }

        var timeSinceLastJumpValue = poseData.TimeSinceLastJump;
        player.CharacterEntity.TimeSinceLastJump ??=
            timeSinceLastJumpValue >= 0 ? Convert.ToUInt16(timeSinceLastJumpValue) : throw new ArgumentOutOfRangeException($"{nameof(poseData.TimeSinceLastJump)} is <0, but we're only allowing >=0. This is bad!");

        //_logger.Warning( "Movement Unknown1: {0:X4} {1:X4} {2:X4} {3:X4} {4:X4}", pkt.UnkUShort1, pkt.UnkUShort2, pkt.UnkUShort3, pkt.UnkUShort4, pkt.LastJumpTimer );

        var resp = new ConfirmedPoseUpdate
                   {
                       PoseData = new MovementPoseData
                                  {
                                      ShortTime = movementInput.ShortTime,
                                      MovementType = MovementDataType.PosRotState,
                                      MovementUnk3 = 0, // ToDo: Find out why this has to be 0; What does it control?
                                      PosRotState = new MovementPosRotState
                                                    {
                                                        Pos = player.CharacterEntity.Position,
                                                        Rot = player.CharacterEntity.Rotation,
                                                        MovementState = (short)player.CharacterEntity.MovementStateContainer.MovementState // ToDo: This was ushort previously!
                                                    },
                                      Velocity = player.CharacterEntity.Velocity,
                                      JetpackEnergy = poseData.JetpackEnergy,
                                      GroundTimePositiveAirTimeNegative = poseData.GroundTimePositiveAirTimeNegative, // Somehow affects gravity
                                      TimeSinceLastJump = poseData.TimeSinceLastJump,
                                      HaveDebugData = 0
                                  },
                       NextShortTime = unchecked((ushort)(movementInput.ShortTime + 90))
                   };

        // ToDo: Set "Aim" property of response if the input had the respective flag
        // ToDo: Handle JetPackEnergy changes / add to CharacterEntity class

        client.NetChannels[ChannelType.UnreliableGss].SendIAero(resp, entityId, 0, typeof(Events));

        if (player.CharacterEntity.TimeSinceLastJump.HasValue && poseData.TimeSinceLastJump > player.CharacterEntity.TimeSinceLastJump.Value)
        {
            player.Jump();
        }
    }

    [MessageID((byte)Commands.SetMovementSimulation)]
    public void SetMovementSimulation(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.SetMovementSimulation
        LogMissingImplementation<BaseController>(nameof(SetMovementSimulation), entityId, packet, _logger);
    }

    [MessageID((byte)Commands.BagInventorySettings)]
    public void BagInventorySettings(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.BagInventorySettings
        LogMissingImplementation<BaseController>(nameof(BagInventorySettings), entityId, packet, _logger);

        packet.Unpack<BagInventorySettings>();
    }

    [MessageID((byte)Commands.SetSteamUserId)]
    public void SetSteamUserId(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var setSteamIdPacket = packet.Unpack<SetSteamUserId>();
        player.SteamUserId = setSteamIdPacket.SteamUserId;
        _logger.Verbose("Entity {0:x8} Steam user id (Aero): {1}", entityId, player.SteamUserId);
        //var conventional = packet.Read<SetSteamIdRequest>();
        //_logger.Verbose("Packet Data: {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
        //_logger.Verbose("Entity {0:x8} Steam user id (conventional): {1}", entityId, conventional.SteamId);
    }
}