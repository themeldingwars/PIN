using GameServer.Data.SDB.Records.apt;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Aptitude;

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