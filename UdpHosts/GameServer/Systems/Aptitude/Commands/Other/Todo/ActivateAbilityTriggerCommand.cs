using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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