using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Shared.Content.Spawns;
using Shared.Content.Zones;

namespace Shared.Content;

/// <summary>
///     Checks what the JSON reader can't: unique ids, the id format and how fields relate. References into the SDB are
///     the game server's job, the SDB can't be shipped with the content.
/// </summary>
public static partial class ContentValidator
{
    public static IReadOnlyList<ContentProblem> Validate(ZoneContent zone, IReadOnlyCollection<string> spawnTables)
    {
        var problems = new List<ContentProblem>();
        Check(zone.ZoneId, ContentKinds.Deployables, zone.Deployables, problems, d => d.Id);
        Check(zone.ZoneId, ContentKinds.Outposts, zone.Outposts, problems, null);
        Check(zone.ZoneId, ContentKinds.Meldings, zone.Meldings, problems, m => m.Id);
        Check(zone.ZoneId, ContentKinds.Encounters, zone.Encounters, problems, e => e.Id);
        Check(zone.ZoneId, ContentKinds.Regions, zone.Regions, problems, r => r.Id);

        foreach (var melding in zone.Meldings.Where(m => m.ControlPoints.Count != m.Offsets.Count))
        {
            problems.Add(new ContentProblem(zone.ZoneId, ContentKinds.Meldings.Name, melding.Key, $"{melding.ControlPoints.Count} control points but {melding.Offsets.Count} offsets"));
        }

        foreach (var deployable in zone.Deployables.Where(d => d.Orientation.LengthSquared() < 1e-6f))
        {
            problems.Add(new ContentProblem(zone.ZoneId, ContentKinds.Deployables.Name, deployable.Key, "orientation is all zero"));
        }

        if (!(zone.Settings.Population.ActivationRadius > 0))
        {
            problems.Add(new ContentProblem(zone.ZoneId, "zone", null, "population.activation_radius has to be more than 0"));
        }

        foreach (var region in zone.Regions)
        {
            void Add(string message) => problems.Add(new ContentProblem(zone.ZoneId, ContentKinds.Regions.Name, region.Key, message));

            if (!spawnTables.Contains(region.Table))
            {
                Add($"spawn table {region.Table} doesn't exist");
            }

            switch (region.Shape)
            {
                case CircleShape circle when !(circle.Radius >= 0):
                    Add("radius has to be 0 or more");
                    break;
                case PolygonShape polygon when polygon.Points.Count < 3:
                    Add($"a polygon needs at least 3 points, it has {polygon.Points.Count}");
                    break;
                case PolygonShape polygon when !(polygon.Area() > 0):
                    Add("the polygon's points lie on a line, nothing is inside");
                    break;
            }

            if (region.ActivationRadius is { } activation && !(activation > 0))
            {
                Add("activation_radius has to be more than 0");
            }

            if (region.Population == 0)
            {
                Add("population is 0, nothing would ever spawn");
            }
        }

        return problems;
    }

    public static IReadOnlyList<ContentProblem> Validate(IReadOnlyList<SpawnTableFile> files)
    {
        var kind = ContentKinds.SpawnTables.Name;
        var problems = new List<ContentProblem>();

        // Ids are unique across all files, regions refer to tables without naming the file
        var tables = files.SelectMany(file => file.Tables.Select(table => (file.Family, Table: table))).ToList();
        foreach (var duplicate in tables.GroupBy(t => t.Table.Id).Where(g => g.Count() > 1))
        {
            problems.Add(new ContentProblem(null, kind, duplicate.Key, $"id used {duplicate.Count()} times, in {string.Join(", ", duplicate.Select(t => $"{t.Family}.json"))}"));
        }

        foreach (var (_, table) in tables)
        {
            void Add(string message) => problems.Add(new ContentProblem(null, kind, table.Key, message));

            if (!IdFormat().IsMatch(table.Id))
            {
                Add("ids are lowercase letters and digits with dashes, e.g. brontodon-herd");
            }

            CheckRange(table.RespawnSeconds, "respawn_seconds", Add);
            if (table.Level is { } level)
            {
                CheckRange(level, "level", Add);
                if (level.Min == 0 || level.Max > byte.MaxValue)
                {
                    Add("levels go from 1 to 255");
                }
            }

            if (table.Groups.Count == 0)
            {
                Add("has no groups");
            }

            for (var i = 0; i < table.Groups.Count; i++)
            {
                var group = table.Groups[i];
                if (group.Spread is { } groupSpread && !(groupSpread >= 0))
                {
                    Add($"group {i + 1} has a spread below 0");
                }

                if (group.Weight == 0)
                {
                    Add($"group {i + 1} has weight 0, it would never spawn");
                }

                if (group.Members.Sum(m => (long)m.Count.Min) == 0)
                {
                    Add($"group {i + 1} can turn out empty, at least one member needs a count of 1 or more");
                }

                foreach (var member in group.Members)
                {
                    CheckRange(member.Count, $"count of monster {member.Monster} in group {i + 1}", Add);
                }
            }
        }

        return problems;
    }

    private static void CheckRange(NumberRange range, string name, System.Action<string> add)
    {
        if (range.Min > range.Max)
        {
            add($"{name} goes from {range.Min} down to {range.Max}, the smaller number comes first");
        }
    }

    private static void Check<T>(uint zoneId, ContentKind<T> kind, IReadOnlyList<T> entries, List<ContentProblem> problems, System.Func<T, string>? id)
        where T : ContentEntry
    {
        foreach (var duplicate in entries.GroupBy(e => e.Key).Where(g => g.Count() > 1))
        {
            problems.Add(new ContentProblem(zoneId, kind.Name, duplicate.Key, $"id used {duplicate.Count()} times"));
        }

        if (id == null)
        {
            return;
        }

        foreach (var entry in entries.Where(e => !IdFormat().IsMatch(id(e))))
        {
            problems.Add(new ContentProblem(zoneId, kind.Name, entry.Key, "ids are lowercase letters and digits with dashes, e.g. ne-brontodon-01"));
        }
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex IdFormat();
}
