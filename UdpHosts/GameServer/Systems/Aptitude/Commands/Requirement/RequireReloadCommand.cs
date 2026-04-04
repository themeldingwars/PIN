using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireReloadCommand : Command, ICommand
{
    private RequireReloadCommandDef Params;

    public RequireReloadCommand(RequireReloadCommandDef par)
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
            if (Params.Inittime == 1)
            {
                result = character.Character_CombatView.WeaponReloadedProp > context.InitTime;
            }
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireReloadCommand), Params.Id);
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}