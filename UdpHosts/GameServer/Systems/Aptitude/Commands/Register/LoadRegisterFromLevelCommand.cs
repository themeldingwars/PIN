using GameServer.Data.SDB.Records.apt;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class LoadRegisterFromLevelCommand : Command, ICommand
{
    private LoadRegisterFromLevelCommandDef Params;

    public LoadRegisterFromLevelCommand(LoadRegisterFromLevelCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.FromInitiator == 1)
        {
            if (context.Initiator is not CharacterEntity initiator)
            {
                return false;
            }

            context.Register = AbilitySystem.RegistryOp(
                context.Register,
                initiator.Character_BaseController.LevelProp,
                (Operand)Params.Regop);

            return true;
        }

        if (context.Self is not CharacterEntity character)
        {
            return false;
        }

        context.Register = AbilitySystem.RegistryOp(
            context.Register,
            character.Character_BaseController.LevelProp,
            (Operand)Params.Regop);

        return true;
    }
}