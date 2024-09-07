using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class InteractionInProgressCommand : Command, ICommand
{
    private InteractionInProgressCommandDef Params;

    public InteractionInProgressCommand(InteractionInProgressCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}