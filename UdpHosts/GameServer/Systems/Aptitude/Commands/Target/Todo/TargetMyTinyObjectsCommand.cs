using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetMyTinyObjectsCommand : ICommand
{
    private TargetMyTinyObjectsCommandDef Params;

    public TargetMyTinyObjectsCommand(TargetMyTinyObjectsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}