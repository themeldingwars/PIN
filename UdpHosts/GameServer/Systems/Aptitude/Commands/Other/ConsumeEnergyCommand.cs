using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ConsumeEnergyCommand : Command, ICommand
{
    private ConsumeEnergyCommandDef Params;

    public ConsumeEnergyCommand(ConsumeEnergyCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var amount = AbilitySystem.RegistryOp(context.Register, Params.Amount, (Operand)Params.AmountRegop);
        if (amount != 0f)
        {
            if (Params.OnTargets != 0)
            {
                foreach (IAptitudeTarget target in context.Targets)
                {
                    Consume(target, context.CurrentTime, amount);
                }
            }
            else
            {
                Consume(context.Self, context.CurrentTime, amount);
            }
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
    }

    private static void Consume(IAptitudeTarget target, uint time, float amount)
    {
        if (target is CharacterEntity { IsPlayerControlled: true } character)
        {
            character.JetEnergy.Consume(time, amount);
        }
    }
}