﻿namespace GameServer.Enums.GSS.Generic;

public enum Events : byte
{
    EncounterToUIMessage = 32,
    VotekickInitiated = 33,
    VotekickResponded = 34,
    ScoreBoardEnable = 35,
    ScoreBoardInit = 36,
    ScoreBoardSetWinner = 37,
    ScoreBoardClear = 38,
    ScoreBoardAddPlayer = 39,
    ScoreBoardRemovePlayer = 40,
    ScoreBoardUpdatePlayerStats = 41,
    ScoreBoardUpdatePlayerStatsFromStat = 42,
    ScoreBoardUpdatePlayerStatus = 43,
    MatchLoadingState = 44,
    MatchEndAck = 45,
    ServerProfiler_SendNames = 46,
    ServerProfiler_SendFrame = 47,
    TempConsoleMessage = 48,
    ReloadStaticData = 49,
    EncDebugChatMessage = 50,
    SendRadioMessage = 51,
    NpcBehaviorInfo = 52,
    NpcMonitoringLog = 53,
    NpcNavigationInfo = 54,
    NpcHostilityDebugInfo = 55,
    NpcPositionalDebugInfo = 56,
    NpcShapeDebugInfo = 57,
    NpcVoxelInfo = 58,
    DebugDrawInfo = 59,
    NpcDevCmdResponse = 60,
    DevRequestObjectPositions = 61,
    DevRequestSpawnTables = 62,
    DevRequestResourceNodeDebug = 63,
    MissionObjectiveUpdated = 64,
    MissionStatusChanged = 65,
    MissionReturnToChanged = 66,
    MissionsAvailable = 67,
    MissionActivationAck = 68,
    BountyStatusChanged = 69,
    BountyAbortAck = 70,
    BountyActivationAck = 71,
    BountyListActiveAck = 72,
    BountyListActiveDetailsAck = 73,
    BountyListAvailableAck = 74,
    BountyClearAck = 75,
    BountyClearPreviousAck = 76,
    BountyListPreviousAck = 77,
    BountyRerollResponse = 78,
    DisplayUiTrackBounty = 79,
    Achievements = 80,
    AchievementUnlocked = 81,
    TotalAchievementPoints = 82,
    MissionCompletionCounts = 83,
    ContentUnlocked = 84,
    ClientUIAction = 85,
    ArcCompletionHistoryUpdate = 86,
    JobLedgerEntriesUpdate = 87,
    TrackRecipe = 88,
    ClearTrackedRecipe = 89,
    SlotTech = 90,
    InteractableStatusChanged = 91,
    SendTipMessage = 92,
    DebugEventSample = 93,
    DebugLagPlayerSample = 94,
    DebugLagSimulationSample = 95,
    DebugLagRaiaSample = 96,
    DebugEncounterVolumes = 97,
    Trail = 98,
    EncounterDebugNotification = 99,
    EncounterUIScopeIn = 100,
    EncounterUIUpdate = 101,
    EncounterUIScopeOut = 102,
    DisplayUiNotification = 103,
    DisplayMoneyBombBanner = 104,
    SetPreloadPosition = 105,
    PlaySoundId = 106,
    PlaySoundIdAtLocation = 107,
    PlayDialogScriptMessage = 108,
    StopDialogScriptMessage = 109,
    PingMap = 110,
    PingMapMarker = 111,
    EncounterPublicInfo = 112,
    RequestActiveEncounters_Response = 113,
    ShoppingListInit = 114,
    SetClientDailyInfo = 115,
    GlobalCounterUpdate = 116,
    GlobalCounterMilestoneInfo = 117,
    ChatMessageList = 118,
    CurrentLoadoutResponse = 119,
    VendorProductsResponse = 120,
    VendorPurchaseResponse = 121,
    ConductorGlobalAnnouncement = 122
}