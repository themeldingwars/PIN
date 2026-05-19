using AeroMessages.GSS.V66.Character.Controller;
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

    public bool Execute(Context context)
    {
        bool result = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            result = character.Character_CombatController.PermissionFlagsProp.Value.HasFlag(
             (PermissionFlagsData.CharacterPermissionFlags)(1 << Params.Permission));
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}