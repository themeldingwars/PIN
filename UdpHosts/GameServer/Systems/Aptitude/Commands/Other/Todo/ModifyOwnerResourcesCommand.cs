using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyOwnerResourcesCommand : ICommand
{
    private ModifyOwnerResourcesCommandDef Params;

    public ModifyOwnerResourcesCommand(ModifyOwnerResourcesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}