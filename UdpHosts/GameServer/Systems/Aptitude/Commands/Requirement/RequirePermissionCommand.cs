using AeroMessages.GSS.V66.Character.Controller;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

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