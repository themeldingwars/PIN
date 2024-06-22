using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ActivateAbilityTriggerCommand : ICommand
{
    private ActivateAbilityTriggerCommandDef Params;

    public ActivateAbilityTriggerCommand(ActivateAbilityTriggerCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}