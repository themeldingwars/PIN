using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class DropCarryableCommand : Command, ICommand
{
    private DropCarryableCommandDef Params;

    public DropCarryableCommand(DropCarryableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}