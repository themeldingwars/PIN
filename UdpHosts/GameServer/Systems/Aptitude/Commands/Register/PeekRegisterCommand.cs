using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class PeekRegisterCommand : Command, ICommand
{
    private PeekRegisterCommandDef Params;

    public PeekRegisterCommand(PeekRegisterCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        context.Register = AbilitySystem.RegistryOp(context.Register, context.FormerRegister, (Operand)Params.Regop);

        if (context.Register != 0)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
    }
}