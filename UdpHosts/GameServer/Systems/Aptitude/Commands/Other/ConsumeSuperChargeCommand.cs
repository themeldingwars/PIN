using AeroMessages.GSS.Character.Controller;
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
            var currentValue = character.Character_CombatController.SuperChargeProp.Value;

            var percent = AbilitySystem.RegistryOp(context.Register, Params.Percent, (Operand)Params.PercentRegop);
            var value = percent / 100 * currentValue;

            character.Character_CombatController.SuperChargeProp = new SuperChargeData()
               {
                   Value = currentValue - value,
                   Op = (byte)Operand.ASSIGN,
               };

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