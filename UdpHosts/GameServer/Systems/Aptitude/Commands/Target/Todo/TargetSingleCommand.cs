using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetSingleCommand : Command, ICommand
{
    private TargetSingleCommandDef Params;

    public TargetSingleCommand(TargetSingleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}