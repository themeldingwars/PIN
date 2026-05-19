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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}