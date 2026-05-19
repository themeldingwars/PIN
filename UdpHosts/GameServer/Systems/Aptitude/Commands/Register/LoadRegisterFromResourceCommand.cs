using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromResourceCommand : Command, ICommand
{
    private LoadRegisterFromResourceCommandDef Params;

    public LoadRegisterFromResourceCommand(LoadRegisterFromResourceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // todo: meaning of Params.RegisterVal_0 - RegisterVal_10
        if (context.Self is not CharacterEntity character)
        {
            return false;
        }

        context.Register = AbilitySystem.RegistryOp(
            context.Register,
            character.Player.Inventory.GetResourceQuantity(Params.ResourceId),
            (Operand)Params.Regop);

        return true;
    }
}