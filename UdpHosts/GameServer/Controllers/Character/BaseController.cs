using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Controller;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Character.View;
using AeroMessages.GSS.V66.Vehicle;
using VController = AeroMessages.GSS.V66.Vehicle.Controller;
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
                                 LevelProp = cd.Level,
                                 EffectiveLevelProp = cd.Level,
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
        var fetchQueueInfoResponse = new FetchQueueInfoResponse
        {
            Succes = 0,
            Queues = new FetchQueueData[]
            {
                new FetchQueueData
                {
                    QueueId = 2121,
                    Qualifies = 1,
                    ChallengeEnabled = 0,
                    Gametype = "campaign",
                    DisplayKeyName = "MATCH_MAP_PROVING_GROUND",
                    DisplayKeyDesc = "MATCH_MAP_PROVING_GROUND_DESC",
                    ZoneId = 1155,
                    MissionId = 497,
                    Certs = new QueueCertsData[]
                    {
                        new QueueCertsData
                        {
                            CertId = 3589,
                            Passed = 1
                        }
                    },
                    Difficulties = new QueueDifficultiesData[]
                    {
                        new QueueDifficultiesData
                        {
                            DifficultyId = 6922,
                            UiString = "INSTANCE_DIFFICULTY_NORMAL",
                            MinLevel = 20,
                            DisplayLevel = 20,
                            MaxSuggestedLevel = 40,
                            DifficultyKey = "NORMAL_MODE",
                            PlayerCount1 = 5,
                            PlayerCount2 = 5,
                            PlayerCount3 = 5,
                            MinPlayers = 1,
                            MaxPlayers = 5
                        },
                        new QueueDifficultiesData
                        {
                            DifficultyId = 7022,
                            UiString = "INSTANCE_DIFFICULTY_CHALLENGE",
                            MinLevel = 20,
                            DisplayLevel = 20,
                            MaxSuggestedLevel = 40,
                            DifficultyKey = "CHALLENGE_MODE",
                            PlayerCount1 = 5,
                            PlayerCount2 = 5,
                            PlayerCount3 = 5,
                            MinPlayers = 1,
                            MaxPlayers = 5
                        },
                        new QueueDifficultiesData
                        {
                            DifficultyId = 9122,
                            UiString = "INSTANCE_DIFFICULTY_HARD",
                            MinLevel = 45,
                            DisplayLevel = 45,
                            MaxSuggestedLevel = 45,
                            DifficultyKey = "HARD_MODE",
                            PlayerCount1 = 5,
                            PlayerCount2 = 5,
                            PlayerCount3 = 5,
                            MinPlayers = 1,
                            MaxPlayers = 5
                        }
                    },
                    RewardsWinnerItems = new QueueRewardsItemData[] { },
                    RewardsWinnerLoots = new QueueRewardsLootData[]
                    {
                        new QueueRewardsLootData { LootTableId = 10133, DifficultyKey = "CHALLENGE_MODE" },
                        new QueueRewardsLootData { LootTableId = 10134, DifficultyKey = "HARD_MODE" },
                        new QueueRewardsLootData { LootTableId = 10132, DifficultyKey = "NORMAL_MODE" }
                    },
                    RewardsLooserItems = new QueueRewardsItemData[] { },
                    RewardsLooserLoots = new QueueRewardsLootData[] { }
                }
            }
        };
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

        var movementInput = packet.Unpack<AeroMessages.GSS.V66.Character.Command.MovementInput>();

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
        var bagInventoryUpdate = new BagInventoryUpdate
        {
            Data = "{\"version\":2,\"bag_types\":[{\"definitions\":[{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]}],\"slots\":[{\"item_guid\":744967187901210112,\"item_sdb_id\":130370,\"quantity\":1},{\"item_guid\":1536551767489147648,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":2186143391660867840,\"item_sdb_id\":113687,\"quantity\":1},{\"item_guid\":2186270670333924352,\"item_sdb_id\":127743,\"quantity\":1},{\"item_guid\":2186143391660874496,\"item_sdb_id\":113718,\"quantity\":1},{\"item_guid\":745117697983358720,\"item_sdb_id\":131698,\"quantity\":1},{\"item_guid\":817027468102777856,\"item_sdb_id\":114074,\"quantity\":1},{\"item_guid\":817158353140239360,\"item_sdb_id\":139488,\"quantity\":1},{\"item_guid\":2186143391660858880,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":2186143391660875264,\"item_sdb_id\":113853,\"quantity\":1},{\"item_guid\":817158353140193280,\"item_sdb_id\":130002,\"quantity\":1},{\"item_guid\":2186143391660863744,\"item_sdb_id\":113552,\"quantity\":1},{\"item_guid\":1536551767489147136,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":817027462130273792,\"item_sdb_id\":114044,\"quantity\":1},{\"item_guid\":2186143391660864000,\"item_sdb_id\":114226,\"quantity\":1},{\"item_guid\":744965437836734720,\"item_sdb_id\":127097,\"quantity\":1},{\"item_guid\":2186265083587334656,\"item_sdb_id\":132178,\"quantity\":1},{\"item_guid\":2186143391660867072,\"item_sdb_id\":114316,\"quantity\":1},{\"item_guid\":2186143391660862976,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":817027462130281472,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2258317732239097344,\"item_sdb_id\":128688,\"quantity\":1},{\"item_guid\":2186143391660865536,\"item_sdb_id\":128730,\"quantity\":1},{\"item_guid\":2258317732239093248,\"item_sdb_id\":136036,\"quantity\":1},{\"item_guid\":2186143391660870144,\"item_sdb_id\":126413,\"quantity\":1},{\"item_guid\":817027468102772992,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186143391660877568,\"item_sdb_id\":114074,\"quantity\":1},{\"item_guid\":744968734894291968,\"item_sdb_id\":140705,\"quantity\":1},{\"item_guid\":2186143391660873728,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":2186143391660862208,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2258317732238628096,\"item_sdb_id\":143309,\"quantity\":1},{\"item_guid\":2186143391660869632,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":1536551767489147392,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":2186143391660878080,\"item_sdb_id\":113520,\"quantity\":1},{\"item_guid\":2258317732239018240,\"item_sdb_id\":131898,\"quantity\":1},{\"item_guid\":2186143391660872960,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186143391660861440,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2186143391660864512,\"item_sdb_id\":113931,\"quantity\":1},{\"item_guid\":745116577399056896,\"item_sdb_id\":134794,\"quantity\":1},{\"item_guid\":2186143391660873472,\"item_sdb_id\":127270,\"quantity\":1},{\"item_guid\":2186143391660868864,\"item_sdb_id\":125725,\"quantity\":1},{\"item_guid\":2186143391660876288,\"item_sdb_id\":128604,\"quantity\":1},{\"item_guid\":745117697983348224,\"item_sdb_id\":132458,\"quantity\":1},{\"item_guid\":745116577399164928,\"item_sdb_id\":114008,\"quantity\":1},{\"item_guid\":817027462130251776,\"item_sdb_id\":128604,\"quantity\":1},{\"item_guid\":2186122473089611264,\"item_sdb_id\":143670,\"quantity\":1},{\"item_guid\":2186143391660868096,\"item_sdb_id\":113858,\"quantity\":1},{\"item_guid\":817027462130284288,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186265083587840256,\"item_sdb_id\":134832,\"quantity\":1},{\"item_guid\":745117697982909696,\"item_sdb_id\":135912,\"quantity\":1},{\"item_guid\":817027462130266624,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2186143391660871424,\"item_sdb_id\":114160,\"quantity\":1},{\"item_guid\":2186143391660878848,\"item_sdb_id\":113838,\"quantity\":1},{\"item_guid\":744965437836734976,\"item_sdb_id\":127953,\"quantity\":1},{\"item_guid\":2186143391660867328,\"item_sdb_id\":114142,\"quantity\":1},{\"item_guid\":2186143391660874752,\"item_sdb_id\":114112,\"quantity\":1},{\"item_guid\":2329134320037442304,\"item_sdb_id\":77367,\"quantity\":1},{\"item_guid\":817027462130287360,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":2186265083587808256,\"item_sdb_id\":132068,\"quantity\":1},{\"item_guid\":2186143391660863232,\"item_sdb_id\":114044,\"quantity\":1},{\"item_guid\":744995722722969856,\"item_sdb_id\":141906,\"quantity\":1},{\"item_guid\":817044963318509568,\"item_sdb_id\":127223,\"quantity\":1},{\"item_guid\":2186143391660873216,\"item_sdb_id\":136576,\"quantity\":1},{\"item_guid\":817027462130271744,\"item_sdb_id\":127270,\"quantity\":1},{\"item_guid\":2186143391660870656,\"item_sdb_id\":114316,\"quantity\":1},{\"item_guid\":817027462130296320,\"item_sdb_id\":127874,\"quantity\":1},{\"item_guid\":817027468102731264,\"item_sdb_id\":127144,\"quantity\":1},{\"item_guid\":817027462130261760,\"item_sdb_id\":114094,\"quantity\":1},{\"item_guid\":744965437836734208,\"item_sdb_id\":125636,\"quantity\":1},{\"item_guid\":2186143391660866560,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":817027462130292224,\"item_sdb_id\":143437,\"quantity\":1},{\"item_guid\":2186143391660862464,\"item_sdb_id\":136576,\"quantity\":1},{\"item_guid\":817027468102746112,\"item_sdb_id\":128000,\"quantity\":1},{\"item_guid\":817034666132829184,\"item_sdb_id\":124671,\"quantity\":1},{\"item_guid\":817027468102775296,\"item_sdb_id\":127144,\"quantity\":1},{\"item_guid\":745116577399087616,\"item_sdb_id\":128689,\"quantity\":1},{\"item_guid\":2186143391660869888,\"item_sdb_id\":127144,\"quantity\":1},{\"item_guid\":2186143391660858368,\"item_sdb_id\":128604,\"quantity\":1},{\"item_guid\":744967187901213440,\"item_sdb_id\":130370,\"quantity\":1},{\"item_guid\":2186143391660868608,\"item_sdb_id\":129312,\"quantity\":1},{\"item_guid\":817027468102749440,\"item_sdb_id\":125683,\"quantity\":1},{\"item_guid\":2186270670332959744,\"item_sdb_id\":127013,\"quantity\":1},{\"item_guid\":2186143391660861696,\"item_sdb_id\":125683,\"quantity\":1},{\"item_guid\":817027462130298880,\"item_sdb_id\":125725,\"quantity\":1},{\"item_guid\":2186143391660857600,\"item_sdb_id\":127874,\"quantity\":1},{\"item_guid\":817027462130283264,\"item_sdb_id\":128730,\"quantity\":1},{\"item_guid\":2186143391660876544,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":817158353140205568,\"item_sdb_id\":135953,\"quantity\":1},{\"item_guid\":2186143391660865024,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2186143391660872448,\"item_sdb_id\":125683,\"quantity\":1},{\"item_guid\":744961658868883968,\"item_sdb_id\":143123,\"quantity\":1},{\"item_guid\":2186143391660868352,\"item_sdb_id\":127874,\"quantity\":1},{\"item_guid\":744967187901204480,\"item_sdb_id\":143123,\"quantity\":1},{\"item_guid\":2186143391660866048,\"item_sdb_id\":136576,\"quantity\":1},{\"item_guid\":2186143391660875776,\"item_sdb_id\":129312,\"quantity\":1},{\"item_guid\":2186143391660871680,\"item_sdb_id\":113873,\"quantity\":1},{\"item_guid\":2186143391660859392,\"item_sdb_id\":126413,\"quantity\":1},{\"item_guid\":744965437836735232,\"item_sdb_id\":128557,\"quantity\":1},{\"item_guid\":817027468102774272,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":2186143391660875008,\"item_sdb_id\":113514,\"quantity\":1},{\"item_guid\":2186143391660870912,\"item_sdb_id\":114166,\"quantity\":1},{\"item_guid\":817158353140259072,\"item_sdb_id\":129272,\"quantity\":1},{\"item_guid\":2186265083587844608,\"item_sdb_id\":132110,\"quantity\":1},{\"item_guid\":744965437836734464,\"item_sdb_id\":126534,\"quantity\":1},{\"item_guid\":2186143391660878336,\"item_sdb_id\":113712,\"quantity\":1},{\"item_guid\":2186143391660866816,\"item_sdb_id\":113996,\"quantity\":1},{\"item_guid\":2258317732239108352,\"item_sdb_id\":132622,\"quantity\":1},{\"item_guid\":2186270670333924096,\"item_sdb_id\":132418,\"quantity\":1},{\"item_guid\":817158353140298752,\"item_sdb_id\":131939,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":142185,\"quantity\":1},{\"item_guid\":817027462130249728,\"item_sdb_id\":129312,\"quantity\":1},{\"item_guid\":1536551767489147904,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":2186143391660861952,\"item_sdb_id\":128730,\"quantity\":1},{\"item_guid\":817027462130253056,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186143391660869376,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":817027468102729984,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":2186143391660876800,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":0,\"quantity\":0},{\"item_guid\":817042546191304448,\"item_sdb_id\":129604,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86696,\"quantity\":12},{\"item_guid\":0,\"item_sdb_id\":77014,\"quantity\":10},{\"item_guid\":0,\"item_sdb_id\":82597,\"quantity\":15},{\"item_guid\":0,\"item_sdb_id\":86709,\"quantity\":903},{\"item_guid\":0,\"item_sdb_id\":117036,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":95086,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":86669,\"quantity\":1487},{\"item_guid\":0,\"item_sdb_id\":75425,\"quantity\":470},{\"item_guid\":0,\"item_sdb_id\":77682,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":77015,\"quantity\":8},{\"item_guid\":0,\"item_sdb_id\":80274,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":123386,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":85679,\"quantity\":7},{\"item_guid\":0,\"item_sdb_id\":82595,\"quantity\":108},{\"item_guid\":0,\"item_sdb_id\":82596,\"quantity\":128},{\"item_guid\":0,\"item_sdb_id\":82598,\"quantity\":174},{\"item_guid\":0,\"item_sdb_id\":77428,\"quantity\":335},{\"item_guid\":0,\"item_sdb_id\":77429,\"quantity\":330},{\"item_guid\":0,\"item_sdb_id\":82604,\"quantity\":20},{\"item_guid\":0,\"item_sdb_id\":77430,\"quantity\":3},{\"item_guid\":0,\"item_sdb_id\":123400,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":78032,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":77643,\"quantity\":16},{\"item_guid\":0,\"item_sdb_id\":82624,\"quantity\":45},{\"item_guid\":0,\"item_sdb_id\":86672,\"quantity\":239},{\"item_guid\":0,\"item_sdb_id\":123217,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86401,\"quantity\":10},{\"item_guid\":0,\"item_sdb_id\":56,\"quantity\":11},{\"item_guid\":0,\"item_sdb_id\":86667,\"quantity\":4978},{\"item_guid\":0,\"item_sdb_id\":95085,\"quantity\":17},{\"item_guid\":0,\"item_sdb_id\":86621,\"quantity\":11814},{\"item_guid\":0,\"item_sdb_id\":77607,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":30298,\"quantity\":1272},{\"item_guid\":0,\"item_sdb_id\":77860,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":124626,\"quantity\":26},{\"item_guid\":0,\"item_sdb_id\":139960,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":139961,\"quantity\":12},{\"item_guid\":0,\"item_sdb_id\":54003,\"quantity\":4220},{\"item_guid\":0,\"item_sdb_id\":86679,\"quantity\":1344},{\"item_guid\":0,\"item_sdb_id\":77343,\"quantity\":14},{\"item_guid\":0,\"item_sdb_id\":81487,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86668,\"quantity\":1560},{\"item_guid\":0,\"item_sdb_id\":86673,\"quantity\":608},{\"item_guid\":0,\"item_sdb_id\":86681,\"quantity\":13},{\"item_guid\":0,\"item_sdb_id\":86682,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86683,\"quantity\":702},{\"item_guid\":0,\"item_sdb_id\":32755,\"quantity\":21},{\"item_guid\":0,\"item_sdb_id\":56835,\"quantity\":76},{\"item_guid\":0,\"item_sdb_id\":77344,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":77345,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":76947,\"quantity\":10},{\"item_guid\":0,\"item_sdb_id\":86699,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":77346,\"quantity\":14},{\"item_guid\":0,\"item_sdb_id\":86703,\"quantity\":10750},{\"item_guid\":0,\"item_sdb_id\":86711,\"quantity\":429},{\"item_guid\":0,\"item_sdb_id\":86713,\"quantity\":2140},{\"item_guid\":0,\"item_sdb_id\":116563,\"quantity\":36},{\"item_guid\":0,\"item_sdb_id\":77403,\"quantity\":7},{\"item_guid\":0,\"item_sdb_id\":124334,\"quantity\":11},{\"item_guid\":0,\"item_sdb_id\":140057,\"quantity\":110},{\"item_guid\":0,\"item_sdb_id\":95083,\"quantity\":9},{\"item_guid\":0,\"item_sdb_id\":95084,\"quantity\":33},{\"item_guid\":0,\"item_sdb_id\":85535,\"quantity\":21},{\"item_guid\":0,\"item_sdb_id\":76978,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":95087,\"quantity\":7},{\"item_guid\":0,\"item_sdb_id\":76979,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":95088,\"quantity\":35},{\"item_guid\":0,\"item_sdb_id\":95089,\"quantity\":9},{\"item_guid\":0,\"item_sdb_id\":76984,\"quantity\":56},{\"item_guid\":0,\"item_sdb_id\":95093,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":76985,\"quantity\":60},{\"item_guid\":0,\"item_sdb_id\":95094,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":77404,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":123353,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":76986,\"quantity\":30},{\"item_guid\":0,\"item_sdb_id\":95095,\"quantity\":3},{\"item_guid\":0,\"item_sdb_id\":95096,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":95097,\"quantity\":26},{\"item_guid\":0,\"item_sdb_id\":95098,\"quantity\":22},{\"item_guid\":0,\"item_sdb_id\":95099,\"quantity\":20},{\"item_guid\":0,\"item_sdb_id\":120974,\"quantity\":14},{\"item_guid\":0,\"item_sdb_id\":142268,\"quantity\":16},{\"item_guid\":0,\"item_sdb_id\":0,\"quantity\":0},{\"item_guid\":0,\"item_sdb_id\":124565,\"quantity\":11},{\"item_guid\":0,\"item_sdb_id\":82577,\"quantity\":547},{\"item_guid\":0,\"item_sdb_id\":77205,\"quantity\":316},{\"item_guid\":0,\"item_sdb_id\":77606,\"quantity\":3},{\"item_guid\":0,\"item_sdb_id\":0,\"quantity\":0},{\"item_guid\":0,\"item_sdb_id\":120622,\"quantity\":1}]},{\"definitions\":[],\"slots\":[]}]}"
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(bagInventoryUpdate, player.CharacterEntity.EntityId, player.PlayerId);
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

    [MessageID((byte)Commands.VehicleCalldownRequest)]
    public void VehicleCalldownRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        Core.Data.EntityGuid vehicleEntityGuid = new(31, client.AssignedShard.CurrentTime, 3670903, 0x01);

        var vehicleCalldownRequest = packet.Unpack<VehicleCalldownRequest>();

        if (vehicleCalldownRequest == null) { return; }

        var vehicleBaseController = new VController.BaseController
        {
            VehicleIdProp = vehicleCalldownRequest.VehicleID,
            ConfigurationProp = new ConfigurationData { Data = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            FlagsProp = new byte[] { 0x41, 0x41, 0x51, 0x41, 0x41, 0x41, 0x3d, 0x3d },
            EngineStateProp = 0,
            PathStateProp = 1,
            OwnerIdProp = new EntityId { Backing = vehicleEntityGuid.Full, ControllerId = Controller.Vehicle, Id = player.PlayerId },
            OwnerNameProp = "",
            OwnerLocalStringProp = 0,
            OccupantIds_0Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_1Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_2Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_3Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_4Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_5Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            DeployableIds_0Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_1Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_2Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_3Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_4Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_5Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_6Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_7Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_8Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_9Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            SnapMountProp = 0,
            SpawnPoseProp = new VController.SpawnPoseData
                            {
                                Position = vehicleCalldownRequest.Position,
                                Rotation = vehicleCalldownRequest.Rotation,
                                Direction = player.CharacterEntity.AimDirection,
                                Time = client.AssignedShard.CurrentTime
                            },
            SpawnVelocityProp = new Vector3 { X = 0, Y = 0, Z = 0 },
            CurrentPoseProp = new CurrentPoseData
                              {
                                  Position = vehicleCalldownRequest.Position,
                                  Rotation = vehicleCalldownRequest.Rotation,
                                  Direction = player.CharacterEntity.AimDirection,
                                  State = 4096, // What state might this be?
                                  Time = client.AssignedShard.CurrentTime
                              },
            ProcessDelayProp = new ProcessDelayData { Unk1 = 15734, Unk2 = 300 },
            HostilityInfoProp = new HostilityInfoData
                                {
                                    Flags = HostilityInfoData.HostilityFlags.Faction,
                                    FactionId = 1,
                                    TeamId = 0,
                                    Unk2 = 0,
                                    Unk3 = 0,
                                    Unk4 = 0
                                },
            PersonalFactionStanceProp = new PersonalFactionStanceData
                                        {
                                            Unk1 = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = new byte[] { 0x43, 0x51, 0x35, 0x64, 0x2f, 0x31, 0x38, 0x49, 0x41, 0x41, 0x41, 0x3d } },
                                            Unk2 = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = new byte[] { 0x38, 0x67, 0x41, 0x67, 0x41, 0x41, 0x44, 0x79, 0x41, 0x41, 0x41, 0x3d } }
                                        },
            CurrentHealthProp = 60684,
            MaxHealthProp = 60684,
            CurrentShieldsProp = 0,
            MaxShieldsProp = 0,
            CurrentResourcesProp = 0,
            MaxResourcesProp = 0,
            WaterLevelAndDescProp = 0,
            SinFlagsProp = 0,
            SinFlagsPrivateProp = 0,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
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
            ScopeBubbleInfoProp = new ScopeBubbleInfoData { Unk1 = 0, Unk2 = 1 },
            ScalingLevelProp = 0
        };

        var vehicleCombatController = new VController.CombatController
        {
            SlottedAbility_0 = 0,
            SlottedAbility_1 = 0,
            SlottedAbility_2 = 0,
            SlottedAbility_3 = 0,
            SlottedAbility_4 = 0,
            SlottedAbility_5 = 34920,
            SlottedAbility_6 = 0,
            SlottedAbility_7 = 0,
            SlottedAbility_8 = 43,

            SlottedAbility_0Prop = 0,
            SlottedAbility_1Prop = 0,
            SlottedAbility_2Prop = 0,
            SlottedAbility_3Prop = 0,
            SlottedAbility_4Prop = 0,
            SlottedAbility_5Prop = 34920,
            SlottedAbility_6Prop = 0,
            SlottedAbility_7Prop = 0,
            SlottedAbility_8Prop = 43,

            StatusEffectsChangeTime_0Prop = 11465,
            StatusEffectsChangeTime_1Prop = 3139,
            StatusEffectsChangeTime_2Prop = 2132,
            StatusEffectsChangeTime_3Prop = 5763,
            StatusEffectsChangeTime_4Prop = 29801,
            StatusEffectsChangeTime_5Prop = 28521,
            StatusEffectsChangeTime_6Prop = 8302,
            StatusEffectsChangeTime_7Prop = 25970,
            StatusEffectsChangeTime_8Prop = 27760,
            StatusEffectsChangeTime_9Prop = 25441,
            StatusEffectsChangeTime_10Prop = 25701,
            StatusEffectsChangeTime_11Prop = 24864,
            StatusEffectsChangeTime_12Prop = 8308,
            StatusEffectsChangeTime_13Prop = 27749,
            StatusEffectsChangeTime_14Prop = 28005,
            StatusEffectsChangeTime_15Prop = 28261,
            StatusEffectsChangeTime_16Prop = 8308,
            StatusEffectsChangeTime_17Prop = 2609,
            StatusEffectsChangeTime_18Prop = 14641,
            StatusEffectsChangeTime_19Prop = 12602,
            StatusEffectsChangeTime_20Prop = 14902,
            StatusEffectsChangeTime_21Prop = 14645,
            StatusEffectsChangeTime_22Prop = 20000,
            StatusEffectsChangeTime_23Prop = 30319,
            StatusEffectsChangeTime_24Prop = 12576,
            StatusEffectsChangeTime_25Prop = 8244,
            StatusEffectsChangeTime_26Prop = 12338,
            StatusEffectsChangeTime_27Prop = 13873,
            StatusEffectsChangeTime_28Prop = 11552,
            StatusEffectsChangeTime_29Prop = 28704,
            StatusEffectsChangeTime_30Prop = 29551,
            StatusEffectsChangeTime_31Prop = 29801,

            StatusEffects_0Prop = null,
            StatusEffects_1Prop = null,
            StatusEffects_2Prop = null,
            StatusEffects_3Prop = null,
            StatusEffects_4Prop = null,
            StatusEffects_5Prop = null,
            StatusEffects_6Prop = null,
            StatusEffects_7Prop = null,
            StatusEffects_8Prop = null,
            StatusEffects_9Prop = null,
            StatusEffects_10Prop = null,
            StatusEffects_11Prop = null,
            StatusEffects_12Prop = null,
            StatusEffects_13Prop = null,
            StatusEffects_14Prop = null,
            StatusEffects_15Prop = null,
            StatusEffects_16Prop = null,
            StatusEffects_17Prop = null,
            StatusEffects_18Prop = null,
            StatusEffects_19Prop = null,
            StatusEffects_20Prop = null,
            StatusEffects_21Prop = null,
            StatusEffects_22Prop = null,
            StatusEffects_23Prop = null,
            StatusEffects_24Prop = null,
            StatusEffects_25Prop = null,
            StatusEffects_26Prop = null,
            StatusEffects_27Prop = null,
            StatusEffects_28Prop = null,
            StatusEffects_29Prop = null,
            StatusEffects_30Prop = null,
            StatusEffects_31Prop = null
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(vehicleBaseController, vehicleEntityGuid.Full, player.PlayerId);
        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(vehicleCombatController, vehicleEntityGuid.Full, player.PlayerId);
    }

    [MessageID((byte)Commands.SetEffectsFlag)]
    public void SetEffectsFlag(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.SetEffectsFlag
        LogMissingImplementation<BaseController>(nameof(SetEffectsFlag), entityId, packet, _logger);

        packet.Unpack<SetEffectsFlag>();
    }

    [MessageID((byte)Commands.ClientQueryInteractionStatus)]
    public void ClientQueryInteractionStatus(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.ClientQueryInteractionStatus
        LogMissingImplementation<BaseController>(nameof(ClientQueryInteractionStatus), entityId, packet, _logger);

        packet.Unpack<ClientQueryInteractionStatus>();
    }

    [MessageID((byte)Commands.ResourceLocationInfosRequest)]
    public void ResourceLocationInfosRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var resourceLocationInfosResponse = new ResourceLocationInfosResponse
        {
            Data = new ResourceLocationInfo[] { },
            Unk = 0x01
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(resourceLocationInfosResponse, player.CharacterEntity.EntityId, player.PlayerId);
    }

    [MessageID((byte)Commands.FriendsListRequest)]
    public void FriendsListRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var friendsListResponse = new FriendsListResponse
        {
            Unk1 = new FriendsListData[]
            {
                new FriendsListData
                {
                    Unk1 = 9162788533740412926,
                    Unk2 = "TestUser1",
                    Unk3 = "",
                    Unk4 = 1,
                    Unk5 = 1427570048,
                    Unk6 = 1
                },
                new FriendsListData
                {
                    Unk1 = 9153042507174448638,
                    Unk2 = "TestUser2",
                    Unk3 = "",
                    Unk4 = 1,
                    Unk5 = 1471686583,
                    Unk6 = 1
                }
            },
            Unk2 = 0
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(friendsListResponse, player.CharacterEntity.EntityId, player.PlayerId);
    }

    [MessageID((byte)Commands.MapOpened)]
    public void MapOpened(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var mapOpened = new GeographicalReportResponse { };

        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(mapOpened, player.CharacterEntity.EntityId, player.PlayerId);
    }
}