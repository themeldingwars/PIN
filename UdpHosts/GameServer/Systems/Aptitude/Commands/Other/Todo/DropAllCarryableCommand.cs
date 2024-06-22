using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DropAllCarryableCommand : ICommand
{
    private DropAllCarryableCommandDef Params;

    public DropAllCarryableCommand(DropAllCarryableCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}