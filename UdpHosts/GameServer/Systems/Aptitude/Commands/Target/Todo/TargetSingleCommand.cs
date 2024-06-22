using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetSingleCommand : ICommand
{
    private TargetSingleCommandDef Params;

    public TargetSingleCommand(TargetSingleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}