using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequestAbilitySelectionCommand : Command, ICommand
{
    private RequestAbilitySelectionCommandDef Params;

    public RequestAbilitySelectionCommand(RequestAbilitySelectionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}