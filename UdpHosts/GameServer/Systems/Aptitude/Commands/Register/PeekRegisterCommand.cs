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

    public bool Execute(Context context)
    {
        context.Register = AbilitySystem.RegistryOp(context.Register, context.FormerRegister, (Operand)Params.Regop);

        return context.Register != 0;
    }
}