using System;
using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetTrimCommand : Command, ICommand
{
    private TargetTrimCommandDef Params;

    public TargetTrimCommand(TargetTrimCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // Keeps <trimSize> targets on the stack, from ability 30187. Guardian Angel - II ; Protect 2 closest allies
        // todo: meaning of Params.Chomp (0 in 298 instances, 1 in 70 instances)
        var trimSize = AbilitySystem.RegistryOp(context.Register, Params.Trimsize, (Enums.Operand)Params.TrimsizeRegop);

        if (Params.Former == 1)
        {
            var targetsToRemove = context.FormerTargets.Count - (int)trimSize;
            if (targetsToRemove < 0)
            {
                Console.WriteLine($"Not enough FormerTargets for TargetTrimCommand, investigate if this is expected");
                targetsToRemove = Math.Abs(targetsToRemove);
            }

            if (Params.FromFront == 1)
                {
                    context.FormerTargets.RemoveBottomN(targetsToRemove);
                }
                else
                {
                    context.FormerTargets.PopN(targetsToRemove);
                }
        }

        if (Params.Current == 1)
        {
            var targetsToRemove = context.Targets.Count - (int)trimSize;
            if (targetsToRemove < 0)
            {
                // 39360 Heavy Turret
                Console.WriteLine($"Not enough Targets for TargetTrimCommand, investigate if this is expected");
                targetsToRemove = Math.Abs(targetsToRemove);
            }

            if (Params.FromFront == 1)
            {
                context.Targets.RemoveBottomN(targetsToRemove);
            }
            else
            {
                context.Targets.PopN(targetsToRemove);
            }
        }

        return true;
    }
}