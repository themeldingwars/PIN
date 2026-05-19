using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

public class SetLookAtTargetCommand : Command, ICommand
{
    private SetLookAtTargetCommandDef Params;

    public SetLookAtTargetCommand(SetLookAtTargetCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // based on ff1189 should act on context.Self
        return true;
    }
}