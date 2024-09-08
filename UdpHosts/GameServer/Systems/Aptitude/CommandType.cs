using System.Diagnostics.CodeAnalysis;

namespace GameServer.Aptitude;

/// <summary>
/// Aptitude Command Types
/// </summary>
[SuppressMessage("StyleCop.CSharp.DocumentationRules",
"SA1602:EnumerationItemsMustBeDocumented",
Justification = "See https://github.com/themeldingwars/Documentation/wiki/Aptitude for cursory overview.")]
public enum CommandType : uint
{
    /// <summary>
    /// Initiate - Active
    /// </summary>
    ActiveInitiation = 1,

    /// <summary>
    /// Impact - Apply Effect
    /// </summary>
    ImpactApplyEffect = 2,

    /// <summary>
    /// Activation - Instant
    /// </summary>
    InstantActivation = 3,

    /// <summary>
    /// Target - Friendlies
    /// </summary>
    TargetFriendlies = 4,

    /// <summary>
    /// Target - Hostiles
    /// </summary>
    TargetHostiles = 5,

    /// <summary>
    /// Target - PBAE
    /// </summary>
    TargetPBAE = 6,

    /// <summary>
    /// Target - Self
    /// </summary>
    TargetSelf = 7,

    /// <summary>
    /// Target - Single
    /// </summary>
    TargetSingle = 8,

    /// <summary>
    /// Requirement - Cooldown
    /// </summary>
    TimeCooldown = 9,

    /// <summary>
    /// Feedback - Audio
    /// </summary>
    AudioFeedback = 10,

    /// <summary>
    /// Modifier - Damage Dealt
    /// </summary>
    DmgDealtEffect = 11,

    /// <summary>
    /// Modifier - Damage Taken
    /// </summary>
    DmgTakenEffect = 12,

    /// <summary>
    /// Modifier - Forward Speed
    /// </summary>
    FwdRunSpeedEffect = 13,

    /// <summary>
    /// Modifier - Health
    /// </summary>
    HealthModEffect = 14,

    /// <summary>
    /// Impact - Aura
    /// </summary>
    ImpactAura = 15,

    /// <summary>
    /// Impact - Remove Effect
    /// </summary>
    ImpactRemoveEffect = 16,

    /// <summary>
    /// Modifier - Jump Height
    /// </summary>
    JumpHeightEffect = 17,

    /// <summary>
    /// Modifier - Max Health
    /// </summary>
    MaxHealthModEffect = 18,

    /// <summary>
    /// Modifier - Run Speed
    /// </summary>
    RunSpeedEffect = 19,

    /// <summary>
    /// Feedback - Particle Effect (OLD)
    /// </summary>
    ParticleEffect = 20,

    /// <summary>
    /// Activation - Timed
    /// </summary>
    TimedActivation = 22,

    /// <summary>
    /// Target - Possesses Effect
    /// </summary>
    TargetByEffect = 23,

    /// <summary>
    /// Modifier - Max Shields
    /// </summary>
    MaxShieldModEffect = 24,

    /// <summary>
    /// Modifier - Shields
    /// </summary>
    ShieldModEffect = 25,

    /// <summary>
    /// Target - Clear
    /// </summary>
    TargetClear = 26,

    /// <summary>
    /// Target - Cone AE
    /// </summary>
    TargetConeAE = 27,

    /// <summary>
    /// Modifier - Stat
    /// </summary>
    StatModifier = 28,

    /// <summary>
    /// Requirement - Time Duration
    /// </summary>
    TimeDuration = 29,

    /// <summary>
    /// Initiate - Passive
    /// </summary>
    PassiveInitiation = 30,

    /// <summary>
    /// Activation - Staged
    /// </summary>
    StagedActivation = 31,

    /// <summary>
    /// Requirement - Activation
    /// </summary>
    ActivationDuration = 32,

    /// <summary>
    /// Impact - Teleport Instance
    /// </summary>
    TeleportInstance = 33,

    /// <summary>
    /// Reset Trauma
    /// </summary>
    ResetTrauma = 34,

    /// <summary>
    /// Target - Initiator
    /// </summary>
    TargetInitiator = 36,

    /// <summary>
    /// Interrupt
    /// </summary>
    Interrupt = 37,

    /// <summary>
    /// Interaction - Begin
    /// </summary>
    BeginInteraction = 38,

    /// <summary>
    /// Interaction - End
    /// </summary>
    EndInteraction = 39,

    /// <summary>
    /// Target - Filter Interactables
    /// </summary>
    TargetInteractives = 40,

    /// <summary>
    /// Interaction - Mark Interactives
    /// </summary>
    ImpactMarkInteractives = 41,

    /// <summary>
    /// Target - Previous
    /// </summary>
    TargetPrevious = 42,

    /// <summary>
    /// Requirement - Has Targets
    /// </summary>
    HasTargetsDuration = 43,

    /// <summary>
    /// Impact - Damage Targets
    /// </summary>
    InflictDamage = 44,

    /// <summary>
    /// Feedback - Animation
    /// </summary>
    PlayAnimation = 45,

    /// <summary>
    /// Animation - Set Control Parameter
    /// </summary>
    SetAnimCtrlParam = 46,

    /// <summary>
    /// Feedback - Camera Shake
    /// </summary>
    CameraShakeEffect = 47,

    /// <summary>
    /// Object - Create (DEPRECATED)
    /// </summary>
    CreateAbilityObject = 48,

    /// <summary>
    /// Object - Destroy
    /// </summary>
    DestroyAbilityObject = 49,

    /// <summary>
    /// Object - Set Position
    /// </summary>
    SetPosition = 50,

    /// <summary>
    /// Object - Set Orientation
    /// </summary>
    SetOrientation = 51,

    /// <summary>
    /// Object - Set Pitch
    /// </summary>
    SetPitch = 52,

    /// <summary>
    /// Object - Set Yaw
    /// </summary>
    SetYaw = 53,

    /// <summary>
    /// Object - Set Lifespan
    /// </summary>
    SetObjectLifespan = 54,

    /// <summary>
    /// Object - Extend Lifespan
    /// </summary>
    ExtendObjectLifespan = 55,

    /// <summary>
    /// Feedback - Highlight Targets
    /// </summary>
    HighlightTargetsFeedback = 56,

    /// <summary>
    /// Feedback - Particle Effect (AssetId)
    /// </summary>
    ParticleEffectAsset = 57,

    /// <summary>
    /// Duration - Lifespan
    /// </summary>
    LifespanDuration = 58,

    /// <summary>
    /// Impact - Force Push
    /// </summary>
    ForcePush = 59,

    /// <summary>
    /// Update - Yield
    /// </summary>
    UpdateYield = 60,

    /// <summary>
    /// Feedback - Ability Animation
    /// </summary>
    AbilityAnimation = 61,

    /// <summary>
    /// Requirement - Airborne
    /// </summary>
    AirborneDuration = 62,

    /// <summary>
    /// Feedback - Beam Effect
    /// </summary>
    BeamEffect = 63,

    /// <summary>
    /// Set Flags - Combat
    /// </summary>
    CombatFlags = 64,

    /// <summary>
    /// Request Effect
    /// </summary>
    RequestEffect = 65,

    /// <summary>
    /// Requirement - Character State
    /// </summary>
    RequireCState = 67,

    /// <summary>
    /// Requirement - Sprint Modifier
    /// </summary>
    RequireSprintModifier = 68,

    /// <summary>
    /// Movement - Rope pull
    /// </summary>
    RopePull = 69,

    /// <summary>
    /// Impact - Set Target Offset
    /// </summary>
    SetTargetOffset = 70,

    /// <summary>
    /// Requirement - Energy
    /// </summary>
    RequireEnergy = 72,

    /// <summary>
    /// Set Flags - Special Movement Permissions
    /// </summary>
    SpecialMovementPermissions = 73,

    /// <summary>
    /// Impact - Heal Targets
    /// </summary>
    HealDamage = 74,

    /// <summary>
    /// Movement - Bullrush
    /// </summary>
    Bullrush = 75,

    /// <summary>
    /// Inflict Damage from Energy
    /// </summary>
    EnergyToDamage = 76,

    /// <summary>
    /// Requirement - Valid grapple
    /// </summary>
    RequireGrapple = 77,

    /// <summary>
    /// Requirement - Ability Object (DEPRECATED)
    /// </summary>
    RequireAbilityObject = 78,

    /// <summary>
    /// Requirement - Not Moving
    /// </summary>
    RequireMoving = 79,

    /// <summary>
    /// Requirement - Trying to Move
    /// </summary>
    RequireTryingToMove = 80,

    /// <summary>
    /// Requirement - In Range
    /// </summary>
    RequireInRange = 81,

    /// <summary>
    /// Tiny Object - Create
    /// </summary>
    TinyObjectCreate = 82,

    /// <summary>
    /// Tiny Object - Destroy
    /// </summary>
    TinyObjectDestroy = 83,

    /// <summary>
    /// Tiny Object - Update
    /// </summary>
    TinyObjectUpdate = 84,

    /// <summary>
    /// Target - Tiny objects
    /// </summary>
    TargetTinyObject = 85,

    /// <summary>
    /// Requirement - Grapple Attached
    /// </summary>
    RequireGrappleAttached = 86,

    /// <summary>
    /// Movement - Cancel Rope pull
    /// </summary>
    CancelRopePull = 87,

    /// <summary>
    /// Impact - Request battle frame list
    /// </summary>
    RequestBattleFrameList = 88,

    /// <summary>
    /// NPC - Spawn
    /// </summary>
    NPCSpawn = 89,

    /// <summary>
    /// Impact - Apply Impulse
    /// </summary>
    ApplyImpulse = 90,

    /// <summary>
    /// Deployable - Spawn
    /// </summary>
    DeployableSpawn = 91,

    /// <summary>
    /// NPC - Droid Mode Change
    /// </summary>
    NPCDroidModeChange = 92,

    /// <summary>
    /// Feedback - Switch Material
    /// </summary>
    SwitchMaterial = 93,

    /// <summary>
    /// Requirement - Battle Frame
    /// </summary>
    BattleFrameDuration = 94,

    /// <summary>
    /// Requirement - Shooting
    /// </summary>
    ShootingDuration = 95,

    /// <summary>
    /// Requirement - Weapon Armed
    /// </summary>
    RequireWeaponTemplate = 96,

    /// <summary>
    /// Impact - Switch To Weapon
    /// </summary>
    SwitchWeapon = 97,

    /// <summary>
    /// Requirement - Stat Comparison
    /// </summary>
    StatRequirement = 98,

    /// <summary>
    /// Impact - Consume energy
    /// </summary>
    ConsumeEnergy = 99,

    /// <summary>
    /// Target - Class Type
    /// </summary>
    TargetClassType = 100,

    /// <summary>
    /// Target - Difference
    /// </summary>
    TargetDifference = 101,

    /// <summary>
    /// Control - Conditional Branch
    /// </summary>
    ConditionalBranch = 102,

    /// <summary>
    /// Control - Logical Or
    /// </summary>
    LogicOr = 103,

    /// <summary>
    /// Control - Logical Negate
    /// </summary>
    LogicNegate = 104,

    /// <summary>
    /// Control - Return Status
    /// </summary>
    Return = 105,

    /// <summary>
    /// Control - Call
    /// </summary>
    Call = 106,

    /// <summary>
    /// Target Stack - Push
    /// </summary>
    PushTargets = 107,

    /// <summary>
    /// Target Stack - Pop
    /// </summary>
    PopTargets = 108,

    /// <summary>
    /// Target Stack - Peek
    /// </summary>
    PeekTargets = 109,

    /// <summary>
    /// Requirement - Client/Server
    /// </summary>
    RequirementServer = 110,

    /// <summary>
    /// Impact - Toggle Fire Mode
    /// </summary>
    ToggleFireMode = 111,

    /// <summary>
    /// Impact - Fire Projectile
    /// </summary>
    FireProjectile = 112,

    /// <summary>
    /// Requirement - Server Confirmed
    /// </summary>
    RequireServerConfirmed = 113,

    /// <summary>
    /// Impact - Apply Freeze
    /// </summary>
    ApplyFreeze = 114,

    /// <summary>
    /// Impact - Climb Ledge
    /// </summary>
    ClimbLedge = 115,

    /// <summary>
    /// Target - Filter By Object Type
    /// </summary>
    TargetByObjectType = 116,

    /// <summary>
    /// Requirement - Aiming in Range
    /// </summary>
    AimRangeDuration = 117,

    /// <summary>
    /// Impact - Sin Acquire
    /// </summary>
    SinAcquire = 118,

    /// <summary>
    /// Target - Filter By Character State
    /// </summary>
    TargetByCharacterState = 119,

    /// <summary>
    /// Requirement - Line Of Sight
    /// </summary>
    RequireLineOfSight = 120,

    /// <summary>
    /// Impact - Copy Initiation Position
    /// </summary>
    CopyInitiationPosition = 122,

    /// <summary>
    /// Requirement - BattleFrame Level
    /// </summary>
    RequireLevel = 123,

    /// <summary>
    /// Requirement - Jump Input
    /// </summary>
    RequireJumped = 124,

    /// <summary>
    /// Requirement - Projectile Hit Slope
    /// </summary>
    RequireProjectileSlope = 125,

    /// <summary>
    /// Spawn Point - Create
    /// </summary>
    CreateSpawnPoint = 126,

    /// <summary>
    /// Target - My NPCs
    /// </summary>
    TargetCharacterNPCs = 127,

    /// <summary>
    /// NPC - Set Behavior
    /// </summary>
    NPCBehaviorChange = 128,

    /// <summary>
    /// Requirement - Aim Mode
    /// </summary>
    RequireAimMode = 130,

    /// <summary>
    /// Weapon - Slot Ammo Type
    /// </summary>
    SlotAmmo = 131,

    /// <summary>
    /// Physics - Additional Actor
    /// </summary>
    AddPhysics = 132,

    /// <summary>
    /// Requirement - Reloaded
    /// </summary>
    RequireReload = 133,

    /// <summary>
    /// Target - Filter Existing Objects
    /// </summary>
    TargetByExists = 134,

    /// <summary>
    /// Interaction - Current Type
    /// </summary>
    InteractionType = 135,

    /// <summary>
    /// Target Stack - Empty
    /// </summary>
    TargetStackEmpty = 136,

    /// <summary>
    /// Interaction - Select Target
    /// </summary>
    SetInteractTarget = 137,

    /// <summary>
    /// Feedback - Perform Emote
    /// </summary>
    PerformEmote = 138,

    /// <summary>
    /// Requirement - Interaction In Progress
    /// </summary>
    InteractionInProgress = 139,

    /// <summary>
    /// Interaction - Set Completion Time
    /// </summary>
    InteractionCompletionTime = 140,

    /// <summary>
    /// Impact - Execute Targets
    /// </summary>
    Execute = 141,

    /// <summary>
    /// Impact - Revive Targets
    /// </summary>
    Revive = 142,

    /// <summary>
    /// Requirement - Possesses Effect
    /// </summary>
    RequireHasEffect = 143,

    /// <summary>
    /// Weapon - Force Reload
    /// </summary>
    ForceReload = 144,

    /// <summary>
    /// Requirement - In Vehicle
    /// </summary>
    RequireInVehicle = 145,

    /// <summary>
    /// Target - Current Vehicle
    /// </summary>
    TargetCurrentVehicle = 146,

    /// <summary>
    /// Target - Owner
    /// </summary>
    TargetOwner = 147,

    /// <summary>
    /// Requirement - Took Damage
    /// </summary>
    RequireTookDamage = 148,

    /// <summary>
    /// Self - Modify Resource
    /// </summary>
    ModifyOwnerResources = 149,

    /// <summary>
    /// Self - Modify Permission
    /// </summary>
    ModifyPermission = 150,

    /// <summary>
    /// Requirement - Permission
    /// </summary>
    RequirePermission = 151,

    /// <summary>
    /// Target - Passengers
    /// </summary>
    TargetPassengers = 152,

    /// <summary>
    /// Target - Squadmates
    /// </summary>
    TargetSquadmates = 153,

    /// <summary>
    /// Apply - Slot Ability
    /// </summary>
    SlotAbility = 154,

    /// <summary>
    /// Target - Trim List
    /// </summary>
    TargetTrim = 155,

    /// <summary>
    /// Feedback - Melding Hud FX
    /// </summary>
    MeldingHudFx = 156,

    /// <summary>
    /// Weapon Damage - Set
    /// </summary>
    SetWeaponDamage = 157,

    /// <summary>
    /// OverTime - Consume Energy
    /// </summary>
    ConsumeEnergyOverTime = 158,

    /// <summary>
    /// Impact - Request ability selection
    /// </summary>
    RequestAbilitySelection = 159,

    /// <summary>
    /// NPC - Despawn
    /// </summary>
    NPCDespawn = 160,

    /// <summary>
    /// Feedback - Audio Volume
    /// </summary>
    AudioVolumeFeedbackDef = 161,

    /// <summary>
    /// Impact - Restock Ammo
    /// </summary>
    RestockAmmo = 162,

    /// <summary>
    /// Register - Set Value
    /// </summary>
    SetRegister = 163,

    /// <summary>
    /// Register - Load From Bonus Track
    /// </summary>
    LoadRegisterFromBonus = 164,

    /// <summary>
    /// Bonus - Track Value Greater Than
    /// </summary>
    BonusGreaterThan = 165,

    /// <summary>
    /// Target - Filter NPCs
    /// </summary>
    TargetByNPC = 166,

    /// <summary>
    /// Impact - Toggle Status Effect
    /// </summary>
    ImpactToggleEffect = 167,

    /// <summary>
    /// Feedback - Ability Toggled
    /// </summary>
    AbilityToggled = 168,

    /// <summary>
    /// Deployable - Previewed
    /// </summary>
    DeployableCalldown = 169,

    /// <summary>
    /// Turret - Gain Control
    /// </summary>
    TurretControl = 170,

    /// <summary>
    /// Impact - Bombardment
    /// </summary>
    Bombardment = 171,

    /// <summary>
    /// Requirement - Resource
    /// </summary>
    RequireResource = 172,

    /// <summary>
    /// Impact - Inflict Cooldown
    /// </summary>
    InflictCooldown = 173,

    /// <summary>
    /// Requirement - Movement State
    /// </summary>
    RequireMovestate = 174,

    /// <summary>
    /// Self - Grant Item
    /// </summary>
    GrantOwnerItem = 175,

    /// <summary>
    /// Open the Forge
    /// </summary>
    ShoppingInvitation = 176,

    /// <summary>
    /// Apply - Set Guardian
    /// </summary>
    SetGuardian = 177,

    /// <summary>
    /// Apply - Damage Feedback
    /// </summary>
    DamageFeedback = 178,

    /// <summary>
    /// Requirement - Backstab
    /// </summary>
    RequireBackstab = 179,

    /// <summary>
    /// Feedback - Aim Sway
    /// </summary>
    AimSway = 180,

    /// <summary>
    /// Feedback - Fullscreen Effect
    /// </summary>
    FullscreenFx = 181,

    /// <summary>
    /// Vehicle - Calldown
    /// </summary>
    CalldownVehicle = 182,

    /// <summary>
    /// Impact - Set Projectile Target
    /// </summary>
    SetProjectileTarget = 183,

    /// <summary>
    /// Self - Set Scope Bubble
    /// </summary>
    SetScopeBubble = 184,

    /// <summary>
    /// Melding - Spawn Bubble
    /// </summary>
    MeldingBubble = 186,

    /// <summary>
    /// Apply - Mind Control
    /// </summary>
    MindControl = 187,

    /// <summary>
    /// Requirement - Energy From Target
    /// </summary>
    RequireEnergyFromTarget = 188,

    /// <summary>
    /// Requirement - Resource From Target
    /// </summary>
    RequireResourceFromTarget = 189,

    /// <summary>
    /// Impact - Spawn Loot
    /// </summary>
    SpawnLoot = 190,

    /// <summary>
    /// Feedback - Text Notification
    /// </summary>
    Notification = 191,

    /// <summary>
    /// Requirement - Ability Slotted
    /// </summary>
    AbilitySlotted = 192,

    /// <summary>
    /// Register - Load From Resource
    /// </summary>
    LoadRegisterFromResource = 193,

    /// <summary>
    /// Self - Set Look At Target
    /// </summary>
    SetLookAtTarget = 194,

    /// <summary>
    /// Encounter - Spawn
    /// </summary>
    EncounterSpawn = 195,

    /// <summary>
    /// Feedback - Custom Player Camera
    /// </summary>
    CustomPlayerCamera = 196,

    /// <summary>
    /// Matchmaking Queueing
    /// </summary>
    MatchMakingQueue = 197,

    /// <summary>
    /// Activate Mission
    /// </summary>
    ActivateMission = 198,

    /// <summary>
    /// Audio: Change Audio State
    /// </summary>
    AudioStateChange = 199,

    /// <summary>
    /// Update - Wait
    /// </summary>
    UpdateWait = 200,

    /// <summary>
    /// Register - Load From Stat
    /// </summary>
    LoadRegisterFromStat = 201,

    /// <summary>
    /// Register - Push
    /// </summary>
    PushRegister = 202,

    /// <summary>
    /// Register - Pop
    /// </summary>
    PopRegister = 203,

    /// <summary>
    /// Register - Peek
    /// </summary>
    PeekRegister = 204,

    /// <summary>
    /// Control - While Loop
    /// </summary>
    WhileLoop = 205,

    /// <summary>
    /// Movement - Linear Slide
    /// </summary>
    MovementSlide = 206,

    /// <summary>
    /// Requirement - Energy By Range
    /// </summary>
    RequireEnergyByRange = 207,

    /// <summary>
    /// Network - Stealth
    /// </summary>
    NetworkStealth = 208,

    /// <summary>
    /// Cooldown - Reset Abilities
    /// </summary>
    ResetCooldowns = 209,

    /// <summary>
    /// Reward - Assist
    /// </summary>
    RewardAssist = 210,

    /// <summary>
    /// Deployable - Upgrade
    /// </summary>
    DeployableUpgrade = 211,

    /// <summary>
    /// NPC - Equip Monster Type
    /// </summary>
    NPCEquipMonster = 212,

    /// <summary>
    /// Requirement - Army
    /// </summary>
    RequireArmy = 213,

    /// <summary>
    /// Impact - Set Hostility
    /// </summary>
    SetHostility = 214,

    /// <summary>
    /// Movement - Teleport
    /// </summary>
    Teleport = 215,

    /// <summary>
    /// Target - From Status Effect
    /// </summary>
    TargetFromStatusEffect = 216,

    /// <summary>
    /// Self - Gain Temporary Equipment
    /// </summary>
    TemporaryEquipment = 217,

    /// <summary>
    /// Requirement - Client Local Hostility
    /// </summary>
    RequireLocalHostility = 218,

    /// <summary>
    /// Self - Set Emissive Color
    /// </summary>
    SetCharEmissive = 219,

    /// <summary>
    /// Requirement - Damage Response
    /// </summary>
    RequireDamageResponse = 220,

    /// <summary>
    /// Target - Filter By Damage Response
    /// </summary>
    TargetByDamageResponse = 221,

    /// <summary>
    /// Movement - Lock Orientation
    /// </summary>
    OrientationLock = 222,

    /// <summary>
    /// Register - Load Module Power
    /// </summary>
    LoadRegisterFromModulePower = 223,

    /// <summary>
    /// Requirement - Forced Movement
    /// </summary>
    ForcedMovementDuration = 224,

    /// <summary>
    /// Impact - Activate Spawn Table
    /// </summary>
    ActivateSpawnTable = 225,

    /// <summary>
    /// Sin Link Reveal
    /// </summary>
    SinLinkReveal = 226,

    /// <summary>
    /// Sin Link Unlock
    /// </summary>
    SinLinkUnlock = 227,

    /// <summary>
    /// Resource Node Scan
    /// </summary>
    ResourceNodeScanDef = 229,

    /// <summary>
    /// Resource Node Beacon Placer
    /// </summary>
    ResourceNodeBeaconCalldown = 230,

    /// <summary>
    /// Feedback - UI State
    /// </summary>
    UiState = 231,

    /// <summary>
    /// Feedback - Set Shader Param
    /// </summary>
    SetShaderParam = 232,

    /// <summary>
    /// Map Open
    /// </summary>
    MapOpen = 233,

    /// <summary>
    /// UI - Send Tip Message
    /// </summary>
    SendTipMessage = 234,

    /// <summary>
    /// Movement - Fall To Ground
    /// </summary>
    FallToGround = 235,

    /// <summary>
    /// Map Close
    /// </summary>
    MapClose = 236,

    /// <summary>
    /// Network - Bullet Time
    /// </summary>
    BulletTime = 237,

    SetPoweredState = 238,
    NamedVariableAssign = 239,
    LoadRegisterFromNamedVar = 240,
    FireUiEvent = 241,
    CalculateTrajectory = 242,
    RegisterComparison = 243,
    ConsumeItem = 244,
    RegisterRandom = 245,
    ResourceAreaCreateNode = 246,
    SetGliderParametersDef = 247,
    SetHoverParametersDef = 248,
    SetVisualInfoIndex = 249,
    DrawScanPlotDef = 250,
    UiNamedVariable = 251,
    SetRespawnFlags = 252,
    VehicleCalldown = 253,
    EncounterSignal = 254,
    RequireNeedsAmmo = 255,
    TargetByNPCType = 256,
    LoadRegisterFromItemStat = 257,
    HostilityHack = 258,
    DetonateProjectiles = 259,
    SpectatorEvent = 260,
    RequireBulletHit = 261,
    LoadRegisterFromDamage = 262,
    TargetSwap = 264,
    ApplyPermanentEffect = 265,
    ModifyHostility = 266,
    DecoyModel = 267,
    RegisterAbilityTrigger = 268,
    SetWeaponDamageType = 269,
    RequireNotRespawned = 270,
    SINPermanent = 271,
    ParticleEffectParameter = 273,
    AbilityFinished = 274,
    TargetFilterMovestate = 275,
    RequireAbilityPhysics = 276,
    ClearHostility = 277,
    UpdateSpawnTable = 278,
    ApplyAssetOverrides = 279,
    TargetByHostility = 280,
    SetWeaponColor = 281,
    RegisterClientProximity = 282,
    SpawnPhysicsProp = 283,
    ApplySinCard = 284,
    UnlockOrnaments = 285,
    DropCarryable = 286,
    RequireSinAcquired = 287,
    EquipLoadout = 288,
    RequireHasEffectTag = 289,
    TargetByEffectTag = 290,
    RemoveEffectByTag = 291,
    RegisterEffectTagTrigger = 292,
    ReplenishableDuration = 293,
    ReplenishEffectDuration = 294,
    ConsumeSuperCharge = 295,
    RequireSuperCharge = 296,
    ActivateAbilityTrigger = 297,
    TargetByHealth = 298,
    RegisterHitTagTypeTrigger = 299,
    SetBoneScale = 300,
    AVMaterialOverride = 301,
    LogicOrChain = 302,
    LogicAndChain = 303,
    RegisterMovementEffect = 304,
    SetFocalPoint = 305,
    AuthorizeTerminal = 306,
    TemporaryEquipmentStatMapping = 307,
    ParticleEffectMultiplex = 308,
    LoadRegisterFromLevel = 309,
    UnlockCerts = 310,
    UnlockPatterns = 311,
    UnlockTitles = 312,
    UnlockWarpaints = 313,
    UnlockDecals = 314,
    AwardRedBeans = 315,
    SetDefaultDamageBonus = 316,
    RequireEquippedItem = 317,
    SetFireMode = 318,
    CarryableObjectSpawn = 319,
    UnlockVisualOverrides = 320,
    RequireItemAttribute = 321,
    AddLootTable = 322,
    UpdateWaitAndFireOnce = 323,
    RequireZoneType = 325,
    SetInteractionType = 326,
    UnpackItem = 327,
    TargetOwnedDeployables = 328,
    RemovePermanentEffect = 329,
    RequireLootStore = 330,
    TargetBySinVulnerable = 331,
    LocalParticleEffect = 332,
    ModifyDamageByType = 333,
    ModifyDamageByFaction = 334,
    ModifyDamageByHeadshot = 335,
    UnlockHeadAccessories = 336,
    RequireDamageType = 337,
    RequireWeaponArmed = 338,
    ModifyDamageByTarget = 339,
    ModifyDamageByMyHealth = 340,
    ModifyDamageByTargetHealth = 341,
    ModifyDamageByTargetDamageResponse = 342,
    AddAccountGroup = 343,
    RequireInitiatorExists = 344,
    RegisterTimedTrigger = 345,
    Taunt = 346,
    ClientInput = 347,
    StartArc = 348,
    RequestArcJobs = 349,
    UnlockBattleframes = 350,
    AddAppendageHealthPool = 352,
    RequireSquadLeader = 353,
    RequireHasCertificate = 354,
    DropAllCarryable = 355,
    SlotTech = 356,
    RemoteAbilityCall = 357,
    ShowItemIcon = 358,
    RequireInCombat = 359,
    LoadLocalLevel = 360,
    RequireHasItem = 361,
    AttachObject = 362,
    MountVehicle = 363,
    RequireIsNPC = 364,
    TargetAiTarget = 365,
    ModifyDamageForInflict = 366,
    ApplyAmmoRider = 367,
    TargetFilterByRange = 368,
    OverrideCollision = 369,
    RequireHasUnlock = 370,
    UnlockContent = 371,
    ShowRewardScreen = 372,
    RemoveRagdollBody = 373,
    RegisterLoadScale = 374,
    EnableInteraction = 375,
    ReputationModifier = 376,
    HostilityOverride = 377,
    AddFactionReputation = 378,
    MovementFacing = 379,
    RequireFriends = 380,
    TargetFilterBySinAcquired = 381,
    RequireMovementFlags = 382,
    ItemAttributeModifier = 383,
    MovementTether = 384,
    DamageItemSlot = 385,
    TargetMyTinyObjects = 386,
    RegisterLoadFromWeapon = 387,
    ApplyClientStatusEffect = 388,
    RemoveClientStatusEffect = 389,
    RequireItemDurability = 390,
    RequireEliteLevel = 391,
    RequireCAISState = 392,
    InflictHitFeedback = 393,
    RepositionClones = 394,
    ApplyUnlock = 395,
    RequireAppliedUnlock = 396,
    RequireHeadshot = 397,
    DisableChatBubble = 398,
    DisableHealthAndIcon = 399,
    AddInitiatorToStatusEffect = 400,
    RemoveInitiatorFromStatusEffect = 401,
    ForceRespawn = 402,
    ReduceCooldowns = 403,
    RequireArcActive = 404,
    AttemptToCalldownVehicle = 405
}