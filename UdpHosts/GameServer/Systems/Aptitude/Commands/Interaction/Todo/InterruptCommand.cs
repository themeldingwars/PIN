using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class InterruptCommand : Command, ICommand
{
    private InterruptCommandDef Params;

    public InterruptCommand(InterruptCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}