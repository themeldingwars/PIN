using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.apt;
using GameServer.Enums;
using SharpCompress;

namespace GameServer.Aptitude;

public class LoadRegisterFromItemStatCommand : Command, ICommand
{
    private LoadRegisterFromItemStatCommandDef Params;

    public LoadRegisterFromItemStatCommand(LoadRegisterFromItemStatCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (Params.FromTarget == 1)
        {
            if (context.Targets.Count > 0)
            {
                target = context.Targets.Peek();
            }
            else
            {
                Console.WriteLine($"LoadRegisterFromItemStatCommand has FromTarget specified but we have no target, is something wrong?");
                return true;
            }
        }

        if (Params.FromInitiator == 1)
        {
            target = context.Initiator;
        }

        if (target.GetType() != typeof(Entities.Character.CharacterEntity))
        {
            Console.WriteLine($"LoadRegisterFromItemStatCommand target is not a Character, is something wrong?");
            return true;
        }

        var character = target as Entities.Character.CharacterEntity;

        float prevValue = context.Register;
        float statValue = character.GetItemAttribute(Params.Stat);
        context.Register = AbilitySystem.RegistryOp(prevValue, statValue, (Operand)Params.Regop);

        if (true)
        {
            var statInfo = SDBInterface.GetAttributeDefinition((uint)Params.Stat);
            Console.WriteLine($"LoadRegisterFromItemStatCommand: ({prevValue}, {statValue} ({statInfo.Name.TrimNulls()}), {(Operand)Params.Regop}) => {context.Register}");
        }

        return true;
    }
}