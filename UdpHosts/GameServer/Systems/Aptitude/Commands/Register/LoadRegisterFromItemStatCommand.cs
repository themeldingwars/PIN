using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromItemStatCommand : Command, ICommand
{
    private LoadRegisterFromItemStatCommandDef Params;

    public LoadRegisterFromItemStatCommand(LoadRegisterFromItemStatCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
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
                Logger.Warning("{Command} {CommandId} has FromTarget specified but we have no target, is something wrong?", nameof(LoadRegisterFromItemStatCommand), Params.Id);
                result.SetPass();
                return;
            }
        }

        if (Params.FromInitiator == 1)
        {
            target = context.Initiator;
        }

        if (target is not CharacterEntity character)
        {
            Logger.Warning("{Command} {CommandId} target is not a Character, is something wrong?", nameof(LoadRegisterFromItemStatCommand), Params.Id);
            result.SetPass();
            return;
        }

        float prevValue = context.Register;
        float statValue = character.GetItemAttribute(Params.Stat);
        context.Register = AbilitySystem.RegistryOp(prevValue, statValue, (Operand)Params.Regop);

        if (true)
        {
            var statInfo = SDBInterface.GetAttributeDefinition((uint)Params.Stat);
            Logger.Debug("{Command} {CommandId}: ({prevValue}, {statValue} ({statName}), {op}) => {register}", nameof(LoadRegisterFromItemStatCommand), Params.Id, prevValue, statValue, statInfo.Name.Trim(), (Operand)Params.Regop, context.Register);
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}