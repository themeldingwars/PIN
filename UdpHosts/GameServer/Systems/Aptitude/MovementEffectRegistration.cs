namespace GameServer.Systems.Aptitude;

/// <summary>
/// A movement-state binding made by <c>RegisterMovementEffectCommand</c>: while the character is in
/// <see cref="MovestateIndex"/> (and sprinting, when <see cref="RequireSprint"/> is set) the ability system
/// keeps <see cref="StatusfxId"/> applied to it. Lives exactly as long as the effect that carried the command.
/// </summary>
public class MovementEffectRegistration
{
    public ulong CharacterEntityId;
    public uint StatusfxId;
    public byte MovestateIndex;
    public bool RequireSprint;
}
