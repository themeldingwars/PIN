using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Effect;

public class RegisterEffectTagTriggerCommand : Command, ICommand
{
    private RegisterEffectTagTriggerCommandDef Params;

    public RegisterEffectTagTriggerCommand(RegisterEffectTagTriggerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}