using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequirePermissionCommand : Command, ICommand
{
    private RequirePermissionCommandDef Params;

    public RequirePermissionCommand(RequirePermissionCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        bool cmdResult = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            cmdResult = character.Character_CombatController.PermissionFlagsProp.Value.HasFlag(
             (PermissionFlagsData.CharacterPermissionFlags)(1 << Params.Permission));
        }

        if (Params.Negate == 1)
        {
            cmdResult = !cmdResult;
        }

        if (cmdResult)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}