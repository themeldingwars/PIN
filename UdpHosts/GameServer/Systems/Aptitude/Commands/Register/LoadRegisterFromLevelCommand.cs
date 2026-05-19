using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromLevelCommand : Command, ICommand
{
    private LoadRegisterFromLevelCommandDef Params;

    public LoadRegisterFromLevelCommand(LoadRegisterFromLevelCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.FromInitiator == 1)
        {
            if (context.Initiator is not CharacterEntity initiator)
            {
                result.SetFail();
                return;
            }

            context.Register = AbilitySystem.RegistryOp(
                context.Register,
                initiator.Character_BaseController.LevelProp,
                (Operand)Params.Regop);

            result.SetPass();
            return;
        }

        if (context.Self is not CharacterEntity character)
        {
            result.SetFail();
            return;
        }

        context.Register = AbilitySystem.RegistryOp(
            context.Register,
            character.Character_BaseController.LevelProp,
            (Operand)Params.Regop);

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}