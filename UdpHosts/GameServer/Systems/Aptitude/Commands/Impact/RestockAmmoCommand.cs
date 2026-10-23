using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class RestockAmmoCommand : Command, ICommand
{
    private RestockAmmoCommandDef Params;

    public RestockAmmoCommand(RestockAmmoCommandDef par)
: base(par)
    {
        Params = par;
    }

    // abilities: 53, 57
    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Targets.Count == 0)
        {
            Logger.Warning("{Command} {CommandId} found no targets to restock ammo for; investigate why the target list is empty", nameof(RestockAmmoCommand), Params.Id);
            result.SetPass();
            return;
        }

        foreach (IAptitudeTarget target in context.Targets)
        {
            if (target is CharacterEntity character)
            {
                context.Shard.WeaponSim.RestockAmmo(character);
            }
        }

        result.SetPass();
        return;
    }
}
