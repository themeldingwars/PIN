using System;
using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

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

        Logger.Debug("{Command} {CommandId}: ({prevValue}, {randValue} ({Min} - {Max}), {op}) => {register}", nameof(RegisterRandomCommand), Params.Id, prevValue, randValue, Params.MinValue, Params.MaxValue, (Operand)Params.Regop, context.Register);

        return true;
    }
}