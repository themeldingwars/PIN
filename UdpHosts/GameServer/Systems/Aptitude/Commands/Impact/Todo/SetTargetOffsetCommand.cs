using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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