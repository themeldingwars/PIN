using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RegisterAbilityTriggerCommand : Command, ICommand
{
    private RegisterAbilityTriggerCommandDef Params;

    public RegisterAbilityTriggerCommand(RegisterAbilityTriggerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}