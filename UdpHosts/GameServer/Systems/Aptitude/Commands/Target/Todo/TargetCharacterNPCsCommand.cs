using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetCharacterNPCsCommand : ICommand
{
    private TargetCharacterNPCsCommandDef Params;

    public TargetCharacterNPCsCommand(TargetCharacterNPCsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}