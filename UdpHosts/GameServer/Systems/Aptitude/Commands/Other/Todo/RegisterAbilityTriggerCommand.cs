using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RegisterAbilityTriggerCommand : ICommand
{
    private RegisterAbilityTriggerCommandDef Params;

    public RegisterAbilityTriggerCommand(RegisterAbilityTriggerCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}