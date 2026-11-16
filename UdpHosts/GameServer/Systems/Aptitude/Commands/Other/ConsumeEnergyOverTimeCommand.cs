using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ConsumeEnergyOverTimeCommand : Command, ICommand
{
    private ConsumeEnergyOverTimeCommandDef Params;

    public ConsumeEnergyOverTimeCommand(ConsumeEnergyOverTimeCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        uint now = context.CurrentTime;
        if (context.HasEnergyConsumedTime && context.Self is CharacterEntity { IsPlayerControlled: true } character)
        {
            var perSecond = AbilitySystem.RegistryOp(context.Register, Params.Amount, (Operand)Params.AmountRegop);
            var amount = perSecond * unchecked((int)(now - context.EnergyConsumedTime)) * 0.001f;
            if (amount != 0f)
            {
                character.JetEnergy.Consume(now, amount);
            }
        }

        context.EnergyConsumedTime = now;
        context.HasEnergyConsumedTime = true;
        result.SetPass();
    }

    public override void Reset(Context context)
    {
    }
}