using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SetGuardianCommand : Command, ICommand
{
    private SetGuardianCommandDef Params;

    public SetGuardianCommand(SetGuardianCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}