using System.Diagnostics.CodeAnalysis;

namespace GameServer.Enums.GSS.Generic;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "https://github.com/themeldingwars/Documentation/wiki/Messages-Generic#client")]
public enum Commands : byte
{
    UIToEncounterMessage = 17,
    ServerProfiler_RequestNames = 18,
    ScheduleUpdateRequest = 19,
    LocalProximityAbilitySuccess = 20,
    RemoteProximityAbilitySuccess = 21,
    TrailRequest = 22,
    RequestLeaveZone = 23,
    RequestLogout = 24,
    RequestEncounterInfo = 25,
    RequestActiveEncounters = 26,
    VotekickRequest = 27,
    VotekickResponse = 28,
    GlobalCounterRequest = 29,
    CurrentLoadoutRequest = 30,
    VendorProductRequest = 31
}