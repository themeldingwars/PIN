using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetTrimCommand : ICommand
{
    private TargetTrimCommandDef Params;

    public TargetTrimCommand(TargetTrimCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // todo aptitude: verify regop param
        var trimSize = AbilitySystem.RegistryOp(context.Register, Params.Trimsize, (Enums.Operand)Params.TrimsizeRegop);

        if (Params.Former == 1)
        {
            if (Params.Chomp == 1)
            {
                context.FormerTargets.PopN((int)trimSize);
            }

            if (Params.FromFront == 1)
            {
                context.FormerTargets.RemoveBottomN((int)trimSize);
            }
        }

        if (Params.Current == 1)
        {
            if (Params.Chomp == 1)
            {
                context.Targets.PopN((int)trimSize);
            }

            if (Params.FromFront == 1)
            {
                context.Targets.RemoveBottomN((int)trimSize);
            }
        }

        return true;
    }
}