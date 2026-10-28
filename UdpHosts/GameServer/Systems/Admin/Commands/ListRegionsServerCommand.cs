using System.Globalization;
using System.Linq;
using System.Numerics;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("List the spawn regions near you, the closest first, with what's alive in them.", "regions [<count>]", "regions", "list_regions", "spawn_regions")]
public class ListRegionsServerCommand : ServerCommand
{
    private const int _defaultCount = 10;

    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var count = parameters.Length > 0 ? (int)ParseUIntParameter(parameters[0]) : _defaultCount;
        var position = context.SourcePlayer?.CharacterEntity?.Position ?? Vector3.Zero;
        var regions = context.Shard.Population.Regions;
        if (regions.Count == 0)
        {
            SourceFeedback($"Zone {context.Shard.ZoneId} has no spawn regions", context);
            return;
        }

        var now = context.Shard.CurrentTimeLong;
        foreach (var state in regions.OrderBy(s => Vector3.DistanceSquared(s.Region.Shape.Middle, position)).Take(count))
        {
            var region = state.Region;
            var middle = region.Shape.Middle;
            var alive = state.Groups.SelectMany(group => group.Members).Count(npc => npc.IsAlive);
            var respawn = state.Respawns.Count > 0 ? $", next respawn in {(System.Math.Max(state.Respawns.Min(), now) - now) / 1000} s" : string.Empty;
            SourceFeedback(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{region.Id} ({region.Table}) around {middle.X:0} {middle.Y:0} {middle.Z:0}, {Vector3.Distance(middle, position):0} m away: {(state.IsActive ? "active" : "inactive")}, {state.Groups.Count}/{region.Population} groups, {alive} NPCs alive{respawn}"),
                context);
        }
    }
}
