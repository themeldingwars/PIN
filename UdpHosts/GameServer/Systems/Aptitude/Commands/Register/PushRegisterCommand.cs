using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class PushRegisterCommand : Command, ICommand
{
    private PushRegisterCommandDef Params;

    public PushRegisterCommand(PushRegisterCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        context.FormerRegister = context.Register;
        context.Register = 0;

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}