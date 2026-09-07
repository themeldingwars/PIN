#nullable enable
using GameServer.Entities.Character;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

/// <summary>
/// Resolves which character a character only requirement should look at.
///
/// The requirement commands used to read <see cref="Context.Self"/> and nothing else, which works while a player
/// runs the chain but breaks every chain that is owned by a *non character* entity. Proximity abilities (glider
/// pads, thumper and deployable abilities) run with the deployable as <c>Self</c>, so a check like "the caster is
/// alive" could not be answered: the command returned false, the effect the ability had just applied was removed
/// again, the client re-triggered the proximity ability on its next retry, and the effect was applied and removed
/// over and over. Each of those add/remove pairs rewrote the entity's status effect fields and flushed the
/// entity's changes to every client in range, which is how standing on a boost panel turned into a stalled
/// connection.
///
/// The rule now is: test the entity the requirement names, fall back to the other side of the activation
/// (<see cref="Context.Initiator"/> for chains owned by a deployable, <see cref="Context.Self"/> the other way
/// round) and then to the first character among the current targets. Only a chain that involves no character at
/// all gets "not applicable", which the requirement commands report as a pass instead of a failure.
/// </summary>
internal static class CharacterRequirement
{
    /// <summary>
    /// The character to test, or <c>null</c> when no character is involved in this activation.
    /// </summary>
    public static CharacterEntity? Find(Context context, bool fromInitiator)
    {
        if (AsCharacter(fromInitiator ? context.Initiator : context.Self) is { } named)
        {
            return named;
        }

        // A deployable owned chain still knows which player walked into it.
        if (AsCharacter(fromInitiator ? context.Self : context.Initiator) is { } otherSide)
        {
            return otherSide;
        }

        foreach (var target in context.Targets)
        {
            if (AsCharacter(target) is { } targetCharacter)
            {
                return targetCharacter;
            }
        }

        return null;
    }

    /// <summary>
    /// Logs that a requirement could not be answered because the chain belongs to something that is not a
    /// character. Debug rather than Warning: with a pad or a thumper in the zone this is the normal case, and a
    /// warning per tick per effect is how a busy shard ends up spending its time writing to the console.
    /// </summary>
    public static void LogNotApplicable(Serilog.ILogger logger, string commandName, uint commandId, Context context)
    {
        logger.Debug("[{Command} {CommandId}] is about characters and this chain is owned by {OwnerType}, treating it as satisfied",
            commandName, commandId, context.Self?.GetType().Name ?? "nothing");
    }

    private static CharacterEntity? AsCharacter(IAptitudeTarget? entity) => entity as CharacterEntity;
}
