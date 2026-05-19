using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class SetTargetOffsetCommand : Command, ICommand
{
    private SetTargetOffsetCommandDef Params;

    public SetTargetOffsetCommand(SetTargetOffsetCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}