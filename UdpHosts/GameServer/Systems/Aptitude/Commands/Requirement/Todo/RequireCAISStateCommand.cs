using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireCAISStateCommand : Command, ICommand
{
    private RequireCAISStateCommandDef Params;

    public RequireCAISStateCommand(RequireCAISStateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}