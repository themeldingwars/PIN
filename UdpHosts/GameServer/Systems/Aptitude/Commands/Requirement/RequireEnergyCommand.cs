using System;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireEnergyCommand : Command, ICommand
{
    private RequireEnergyCommandDef Params;

    public RequireEnergyCommand(RequireEnergyCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity { IsPlayerControlled: true } character)
        {
            result.SetPass();
            return;
        }

        var energy = Math.Max(0f, character.JetEnergy.Energy);
        var amount = AbilitySystem.RegistryOp(context.Register, Params.Amount, (Operand)Params.AmountRegop);

        if ((amount < energy) != (Params.Negate != 0))
        {
            result.SetPass();
        }
        else
        {
            result.SetFail(StatusCode.EnergyRequirementFailed);
        }
    }

    public override void Reset(Context context)
    {
    }
}