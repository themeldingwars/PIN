using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromStatCommand : Command, ICommand
{
    private LoadRegisterFromStatCommandDef Params;

    public LoadRegisterFromStatCommand(LoadRegisterFromStatCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // Assumption: a stat the server doesn't know leaves the register as it is
        if (AptitudeStats.Read(context.Self, Params.Stat) is { } value)
        {
            context.Register = AbilitySystem.RegistryOp(context.Register, value, (Operand)Params.Regop);
        }
        else
        {
            Logger.Debug("{Command} {CommandId}: stat {Stat} isn't known for {Self}, the register stays at {Register}", nameof(LoadRegisterFromStatCommand), Params.Id, Params.Stat, context.Self, context.Register);
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
    }
}
