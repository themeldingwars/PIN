using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RegisterEffectTagTriggerCommand : ICommand
{
    private RegisterEffectTagTriggerCommandDef Params;

    public RegisterEffectTagTriggerCommand(RegisterEffectTagTriggerCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}