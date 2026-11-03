using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireNeedsAmmoCommand : Command, ICommand
{
    private RequireNeedsAmmoCommandDef Params;

    public RequireNeedsAmmoCommand(RequireNeedsAmmoCommandDef par)
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
            cmdResult = (Params.CheckPrimary == 1 && context.Shard.WeaponSim.NeedsAmmo(character, 1))
                || (Params.CheckSecondary == 1 && context.Shard.WeaponSim.NeedsAmmo(character, 2));
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