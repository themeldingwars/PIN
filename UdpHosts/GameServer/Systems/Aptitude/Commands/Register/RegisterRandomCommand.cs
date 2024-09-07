using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.apt;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class RegisterRandomCommand : Command, ICommand
{
    private RegisterRandomCommandDef Params;
    private Random rng = new Random();

    public RegisterRandomCommand(RegisterRandomCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        float prevValue = context.Register;
        float randValue = rng.Next((int)Params.MinValue, (int)Params.MaxValue);
        context.Register = AbilitySystem.RegistryOp(prevValue, randValue, (Operand)Params.Regop);

        if (!IsWhole(Params.MinValue) || !IsWhole(Params.MaxValue))
        {
            Console.WriteLine($"RegisterRandomCommand {Params.Id} uses floats with decimals, not handled");
        }

        if (true)
        {
            Console.WriteLine($"RegisterRandomCommand: ({prevValue}, {randValue} ({Params.MinValue} - {Params.MaxValue}), {(Operand)Params.Regop}) => {context.Register}");
        }

        return true;
    }

    private bool IsWhole(float val)
    {
        return val % 1 == 0;
    }
}