using GameServer.Entities.Character;
using GameServer.Extensions;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

/// <summary>
/// CommandType 184 ("Set Scope Bubble").
///
/// Writes <c>ScopeBubbleInfo</c> on the character's base controller, the replicated two value state
/// (<c>Layer</c>, plus a second value nobody has recovered yet) the client keeps for the scope bubble of the
/// entity. The same field is used by melding bubbles, thumpers, loot objects and force shields, and the rows of
/// <c>aptgss::SetScopeBubbleCommandDef</c> that reach it here belong to the glider permission effects, so the
/// meaning of the layer for a character is not recovered yet; what is known is that the field is the server's
/// and that an ability which sets it has to leave the character without one again when it ends.
///
/// Because of that this command only writes what the definition actually says. Rows that carry no layer (which is
/// every row of the table we have so far, the ids are all we recovered) leave the character alone instead of
/// guessing at a layer: an invented value would put a bubble state on everyone gliding or scoped in, and a value
/// the client cannot undo is how the camera was left stuck before.
/// </summary>
public class SetScopeBubbleCommand : Command, ICommand
{
    /// <summary>
    /// Scope bubble layer that means "no bubble at all".
    /// </summary>
    public const uint LayerNone = 0;

    private SetScopeBubbleCommandDef Params;

    public SetScopeBubbleCommand(SetScopeBubbleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Self is not CharacterEntity character)
        {
            Logger.Debug("[{Command} {CommandId}] does nothing because self is {SelfType}",
                nameof(SetScopeBubbleCommand), Params.Id, context.Self?.GetType().Name ?? "nothing");

            return true;
        }

        if (Params.Layer == null)
        {
            if (OnceLog.ShouldLog((nameof(SetScopeBubbleCommand), Params.Id)))
            {
                Logger.Debug("[{Command} {CommandId}] has no layer in its definition, not writing the character's scope bubble",
                    nameof(SetScopeBubbleCommand), Params.Id);
            }

            return true;
        }

        character.SetScopeBubble((uint)Params.Layer, Params.Unk2 ?? character.ScopeBubble.Unk2);

        // Registered as an active so the bubble is taken away again when the effect that set it ends. Clearing
        // (instead of restoring the layer from before the effect) is deliberate: the state is a single value per
        // character, and a layer that comes back on its own after the ability ended is the bug.
        context.Actives.Add(this, new SetScopeBubbleActiveContext());

        return true;
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        if (activeCommandContext is not SetScopeBubbleActiveContext)
        {
            return;
        }

        if (context.Self is CharacterEntity character)
        {
            character.SetScopeBubble(LayerNone, character.ScopeBubble.Unk2);
        }
    }

    /// <summary>
    /// Marks that this effect wrote the scope bubble and has to clear it again.
    /// </summary>
    private class SetScopeBubbleActiveContext : ICommandActiveContext
    {
    }
}
