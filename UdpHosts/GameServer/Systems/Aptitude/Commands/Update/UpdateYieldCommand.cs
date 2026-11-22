using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Update;

public class UpdateYieldCommand : Command, ICommand
{
    private UpdateYieldCommandDef Params;

    public UpdateYieldCommand(UpdateYieldCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var period = (uint)AbilitySystem.RegistryOp(context.Register, Params.Period, (Operand)Params.Regop);
        if (period != 0 && !ProcessSchedule.Due(context, period, Params.Id))
        {
            result.SetPass();
            result.SetYield();
            return;
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
        return;
    }
}
