using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ExecuteCommand : ICommand
{
    private ExecuteCommandDef Params;

    public ExecuteCommand(ExecuteCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // based on ff1189 should act on context.Targets
        return true;
    }
}