using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ResourceNodeScanDefCommand : ICommand
{
    private ResourceNodeScanDefCommandDef Params;

    public ResourceNodeScanDefCommand(ResourceNodeScanDefCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}