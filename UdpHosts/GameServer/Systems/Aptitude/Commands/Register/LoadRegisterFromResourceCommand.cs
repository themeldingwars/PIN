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

    public override void Execute(Context context, ref CommandResult result)
    {
        // todo: meaning of Params.RegisterVal_0 - RegisterVal_10
        if (context.Self is not CharacterEntity character)
        {
            result.SetFail();
            return;
        }

        context.Register = AbilitySystem.RegistryOp(
            context.Register,
            character.Player.Inventory.GetResourceQuantity(Params.ResourceId),
            (Operand)Params.Regop);

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}