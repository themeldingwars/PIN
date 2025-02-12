using System;
using GameServer.Data.SDB.Records.apt;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class RegisterRandomCommand : Command, ICommand
{
    private RegisterRandomCommandDef Params;
    private Random rng = new();

    public RegisterRandomCommand(RegisterRandomCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        float prevValue = context.Register;

        float rand = rng.NextSingle();
        float range = Params.MaxValue - Params.MinValue;
        float randValue = Params.MinValue + (range * rand);
        context.Register = AbilitySystem.RegistryOp(prevValue, randValue, (Operand)Params.Regop);

        Console.WriteLine($"RegisterRandomCommand: ({prevValue}, {randValue} ({Params.MinValue} - {Params.MaxValue}), {(Operand)Params.Regop}) => {context.Register}");

        return true;
    }
}