using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ConsumeSuperChargeCommand : Command, ICommand
{
    private ConsumeSuperChargeCommandDef Params;

    public ConsumeSuperChargeCommand(ConsumeSuperChargeCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            var percent = AbilitySystem.RegistryOp(context.Register, Params.Percent, (Operand)Params.PercentRegop);

            if (Params.IsGainEvent != 0)
            {
                character.Shard.SuperCharge.AddAbilityGain(character, -percent);
                result.SetPass();
                return;
            }

            character.Shard.SuperCharge.ConsumePercentOfCurrent(character, percent);

            result.SetPass();
            return;
        }

        Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(ConsumeSuperChargeCommand), Params.Id);

        result.SetFail();
        return;
    }

    public override void Reset(Context context)
    {
    }
}