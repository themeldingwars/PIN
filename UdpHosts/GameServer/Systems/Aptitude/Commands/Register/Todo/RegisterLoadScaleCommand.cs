using System;
using GameServer.Data.SDB.Records.apt;
using GameServer.Entities.Deployable;
using GameServer.Entities.Thumper;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class RegisterLoadScaleCommand : Command, ICommand
{
    private RegisterLoadScaleCommandDef Params;

    public RegisterLoadScaleCommand(RegisterLoadScaleCommandDef par)
: base(par)
    {
        Params = par;
    }

    // Params.Regop is always 0, the table appears only in prod builds
    // todo: Monster->[min/max]_rand_scale? mostly have -1
    public bool Execute(Context context)
    {
        var scale = context.Self switch
        {
            DeployableEntity d => d.Scale,
            ThumperEntity t    => t.Scale,
            _                  => 0,
        };

        if (scale == 0)
        {
            Console.WriteLine("RegisterLoadScaleCommand fails because Self is not a Deployable or Thumper. If this is happening, we should investigate why.");
            return false;
        }

        context.Register = AbilitySystem.RegistryOp(context.Register, scale, (Operand)Params.Regop);

        return true;
    }
}