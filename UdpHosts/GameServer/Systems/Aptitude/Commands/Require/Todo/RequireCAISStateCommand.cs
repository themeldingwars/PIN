using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireCAISStateCommand : ICommand
{
    private RequireCAISStateCommandDef Params;

    public RequireCAISStateCommand(RequireCAISStateCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}