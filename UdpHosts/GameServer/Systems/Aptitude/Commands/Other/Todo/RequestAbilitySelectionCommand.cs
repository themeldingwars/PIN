using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequestAbilitySelectionCommand : ICommand
{
    private RequestAbilitySelectionCommandDef Params;

    public RequestAbilitySelectionCommand(RequestAbilitySelectionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}