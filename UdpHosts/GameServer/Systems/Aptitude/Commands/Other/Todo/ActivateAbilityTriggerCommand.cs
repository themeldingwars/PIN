using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ActivateAbilityTriggerCommand : Command, ICommand
{
    private ActivateAbilityTriggerCommandDef Params;

    public ActivateAbilityTriggerCommand(ActivateAbilityTriggerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}