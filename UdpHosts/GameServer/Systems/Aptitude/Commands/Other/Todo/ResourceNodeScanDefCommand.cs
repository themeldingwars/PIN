using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ResourceNodeScanDefCommand : Command, ICommand
{
    private ResourceNodeScanDefCommandDef Params;

    public ResourceNodeScanDefCommand(ResourceNodeScanDefCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}