using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireReloadCommand : Command, ICommand
{
    private RequireReloadCommandDef Params;

    public RequireReloadCommand(RequireReloadCommandDef par)
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
            if (Params.Inittime == 1)
            {
                cmdResult = character.Character_CombatView.WeaponReloadedProp > context.InitTime;
            }
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireReloadCommand), Params.Id);
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