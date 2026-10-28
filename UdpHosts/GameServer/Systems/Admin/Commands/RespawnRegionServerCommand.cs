using System.Linq;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Despawn a spawn region's NPCs and skip its respawn timers, it fills again while a player is near.", "region_respawn <regionId> | region_respawn all", "region_respawn", "respawn_region")]
public class RespawnRegionServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Pass a region id, or all", context);
            return;
        }

        var population = context.Shard.Population;
        var ids = parameters[0] == "all" ? population.Regions.Select(state => state.Region.Id).ToList() : [parameters[0]];
        foreach (var id in ids)
        {
            SourceFeedback(population.Respawn(id) ? $"Respawning region {id}" : $"Zone {context.Shard.ZoneId} has no region {id}", context);
        }
    }
}
