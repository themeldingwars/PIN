using System;

namespace GameServer.Entities.Character;

/// <summary>
/// A client ability input (activate / deactivate) recorded on a character, keyed by the resolved ability id
/// so commands running inside the ability can look it up from <c>Context.AbilityId</c>. Deactivation ends the
/// execution that the matching activation started.
/// </summary>
public sealed class AbilityInput
{
    public uint AbilityId { get; init; }

    public Guid ExecutionId { get; init; }

    public uint ActivateTime { get; init; }
}
