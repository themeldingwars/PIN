using System;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

/// <summary>
///     Works out the damage the chain is carrying, in damage points, and puts it on the context.
/// </summary>
public class SetWeaponDamageCommand : Command, ICommand
{
    private SetWeaponDamageCommandDef Params;

    public SetWeaponDamageCommand(SetWeaponDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        float input = 0f;

        if (Params.Lerpenergy == 1)
        {
            Logger.Debug("{Command} {CommandId} has Lerpenergy set, using the bottom of the lerp range because the server tracks no weapon energy", nameof(SetWeaponDamageCommand), Params.Id);
        }
        else if (Params.Lerpfallheight == 1)
        {
            // max(0, the Z the caster was at when the context was initiated minus the Z it has interpolated
            // for the time the chain is running).
            // Fail with status 1 when no init pose
            input = MathF.Max(0f, context.InitBodyPosition.Z - QueryPose.LiveBodyPosition(context).Z);
        }

        // Client: ((max - min) x (input - lerpmin)) / (lerpmax - lerpmin) + min.
        float value = Params.Dmgminvalue + (((Params.Dmgmaxvalue - Params.Dmgminvalue) * (input - Params.Lerpminvalue)) / (Params.Lerpmaxvalue - Params.Lerpminvalue));

        if (Params.Clamplerp == 1)
        {
            value = Math.Clamp(value, Params.Dmgminvalue, Params.Dmgmaxvalue);
        }

        value = AbilitySystem.RegistryOp(context.Register, value, (Operand)Params.DamageRegop);

        if (Params.Set == 1)
        {
            context.DamagePoints = (int)value;
        }
        else if (Params.Multiply == 1)
        {
            context.DamagePoints = (int)(context.DamagePoints * value);
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}
