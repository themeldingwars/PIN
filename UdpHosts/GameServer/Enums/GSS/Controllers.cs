namespace GameServer.Enums.GSS;

public enum Controllers : byte
{
    Generic = 0,
    Character = 1,
    Character_BaseController = 2,
    Character_NPCController = 3,
    Character_MissionAndMarkerController = 4,
    Character_CombatController = 5,
    Character_LocalEffectsController = 6,
    Character_SpectatorController = 7,
    Character_ObserverView = 8,
    Character_EquipmentView = 9,
    Character_AIObserverView = 10,
    Character_CombatView = 11,
    Character_MovementView = 12,
    Character_TinyObjectView = 13,
    Character_DynamicProjectileView = 14,
    Melding = 15,
    Melding_ObserverView = 16,
    MeldingBubble = 17,
    MeldingBubble_ObserverView = 18,
    AreaVisualData = 19,
    AreaVisualData_ObserverView = 20,
    AreaVisualData_ParticleEffectsView = 21,
    AreaVisualData_MapMarkerView = 22,
    AreaVisualData_TinyObjectView = 23,
    AreaVisualData_LootObjectView = 24,
    AreaVisualData_ForceShieldView = 25,
    Vehicle = 26,
    Vehicle_BaseController = 27,
    Vehicle_CombatController = 28,
    Vehicle_ObserverView = 29,
    Vehicle_CombatView = 30,
    Vehicle_MovementView = 31,
    Anchor = 32,
    Anchor_AIObserverView = 33,
    Deployable = 34,
    Deployable_ObserverView = 35,
    Deployable_NPCObserverView = 36,
    Deployable_HardpointView = 37,
    Turret = 38,
    Turret_BaseController = 39,
    Turret_ObserverView = 40,

    Outpost = 44,
    Outpost_ObserverView = 45,

    ResourceNode = 47,
    ResourceNode_ObserverView = 48,

    CarryableObject = 50,
    CarryableObject_ObserverView = 51,

    LootStoreExtension_LootObjectView = 53,

    Generic2 = 251
}