using System;
using GameServer.Data.SDB.Records.apt;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class SetRegisterCommand : Command, ICommand
{
    private SetRegisterCommandDef Params;

    public SetRegisterCommand(SetRegisterCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        float prevValue = context.Register;
        float paramValue = Params.RegisterVal;
        context.Register = AbilitySystem.RegistryOp(prevValue, paramValue, (Operand)Params.Regop);

        if (true)
        {
            Console.WriteLine($"SetRegisterCommand: ({prevValue}, {paramValue}, {(Operand)Params.Regop}) => {context.Register}");
        }

        return true;
    }
}