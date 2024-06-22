using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DropCarryableCommand : ICommand
{
    private DropCarryableCommandDef Params;

    public DropCarryableCommand(DropCarryableCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}