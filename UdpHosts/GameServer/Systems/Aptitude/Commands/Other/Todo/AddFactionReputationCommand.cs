using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AddFactionReputationCommand : Command, ICommand
{
    private AddFactionReputationCommandDef Params;

    public AddFactionReputationCommand(AddFactionReputationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.FactionId != 0 && Params.Amount != 0)
        {
            // todo aptitude: add reputation
        }
        
        return true;
    }
}