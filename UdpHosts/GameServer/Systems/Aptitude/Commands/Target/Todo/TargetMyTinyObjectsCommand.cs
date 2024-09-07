using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetMyTinyObjectsCommand : Command, ICommand
{
    private TargetMyTinyObjectsCommandDef Params;

    public TargetMyTinyObjectsCommand(TargetMyTinyObjectsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}