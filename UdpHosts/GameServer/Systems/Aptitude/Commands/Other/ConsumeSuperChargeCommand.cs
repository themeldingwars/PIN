using System;
using AeroMessages.GSS.V66.Character.Controller;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class ConsumeSuperChargeCommand : Command, ICommand
{
    private ConsumeSuperChargeCommandDef Params;

    public ConsumeSuperChargeCommand(ConsumeSuperChargeCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
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

            return true;
        }

        Console.WriteLine("ConsumeSuperChargeCommand fails because target is not a Character. If this is happening, we should investigate why.");

        return false;
    }
}