using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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