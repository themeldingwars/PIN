using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

/// <summary>
/// CommandType 247 ("Set Glider Parameters").
///
/// Writes the <c>dbcharacter::GliderParameters</c> row the client has to use as the flight model of the
/// character, for as long as the effect that carries this command lasts. Boost pads and the glider abilities
/// share the mechanism: the pad launches you with its own (much more aggressive) profile, and the profile has
/// to be the character's own again the moment the effect ends. That is why this command is an "active":
/// writing the field without handing the previous value back leaves every later glide of that player on the
/// profile of the last pad they jumped off.
///
/// Rows of <c>aptgss::SetGliderParametersCommandDef</c> that carry no value at all (the definition table we
/// have is only complete for a handful of ids) leave the profile alone.
/// </summary>
public class SetGliderParametersCommand : Command, ICommand
{
    private SetGliderParametersCommandDef Params;

    public SetGliderParametersCommand(SetGliderParametersCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (target is CharacterEntity)
        {
            context.Actives.Add(this, new SetGliderParametersActiveContext());
        }
        else
        {
            // Deployable owned chains (a pad running its launch ability) reach this command with the pad as
            // self. There is no glider to reconfigure on a pad and failing the command would take the whole
            // effect down with it, so just step over it.
            Logger.Debug("[{Command} {CommandId}] does nothing because self is {SelfType}",
                nameof(SetGliderParametersCommand), Params.Id, target?.GetType().Name ?? "nothing");
        }

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        if (activeCommandContext is not SetGliderParametersActiveContext active)
        {
            return;
        }

        if (context.Self is not CharacterEntity character)
        {
            return;
        }

        active.PreviousProfileId = character.GliderProfileId;
        active.HasPrevious = true;

        if (Params.Value != null)
        {
            character.SetGliderProfileId((uint)Params.Value);
        }
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        if (activeCommandContext is not SetGliderParametersActiveContext { HasPrevious: true } active)
        {
            return;
        }

        if (context.Self is CharacterEntity character)
        {
            character.SetGliderProfileId(active.PreviousProfileId);
        }
    }

    /// <summary>
    /// The profile the character had before this effect took it over.
    /// </summary>
    private class SetGliderParametersActiveContext : ICommandActiveContext
    {
        public uint PreviousProfileId;
        public bool HasPrevious;
    }
}