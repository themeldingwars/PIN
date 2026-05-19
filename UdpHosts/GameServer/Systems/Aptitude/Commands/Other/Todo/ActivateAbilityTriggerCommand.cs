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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}