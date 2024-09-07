using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyOwnerResourcesCommand : Command, ICommand
{
    private ModifyOwnerResourcesCommandDef Params;

    public ModifyOwnerResourcesCommand(ModifyOwnerResourcesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}