using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

/// <summary>
/// Server-side implementation of apt::RequireAimMode.
/// FireMode_1 is the scoped/ADS state; FireMode_0 is the selected fire mode / underbarrel state.
/// </summary>
public class RequireAimModeCommand : Command, ICommand
{
    private RequireAimModeCommandDef Params;

    public RequireAimModeCommand(RequireAimModeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var character = CharacterRequirement.Find(context, false);
        if (character == null)
        {
            // Requirements used by deployable-owned chains must not tear the effect down merely because
            // the command's Self is the deployable rather than its owning character.
            return true;
        }

        bool isScoped = character.FireMode_1.Mode != 0;
        return Params.Negate == 1 ? !isScoped : isScoped;
    }
}
