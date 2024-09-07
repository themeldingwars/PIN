using GameServer.Data.SDB.Records.apt;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class TargetPBAECommand : Command, ICommand
{
    private TargetPBAECommandDef Params;

    public TargetPBAECommand(TargetPBAECommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // Unused:
        // Params.AimPosOffset
        // Params.MaxTargets
        // Params.UseInitPos
        // Params.MinTargets
        // Params.ScaleOffset
        // Params.IncludeInteractives
        // Params.IgnoreWalls
        // Params.ScaleQuerySize
        // Params.UseBodyPosition
        float radius = AbilitySystem.RegistryOp(context.Register, Params.Radius, (Operand)Params.RadiusRegop);

        if (Params.Filter == 1)
        {
            // 2202 commands have value 0, 5 commands have value 1
            // Probably remove from context.Targets all targets selected using other Params
        }

        if (Params.IncludeSelf == 1)
        {
            context.Targets.Push(context.Self);
        }

        return true;
    }
}