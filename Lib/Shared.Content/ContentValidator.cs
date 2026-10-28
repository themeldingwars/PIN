using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Shared.Content.Zones;

namespace Shared.Content;

/// <summary>
///     Checks what the JSON reader can't: unique ids, the id format and how fields relate. References into the SDB are
///     the game server's job, the SDB can't be shipped with the content.
/// </summary>
public static partial class ContentValidator
{
    public static IReadOnlyList<ContentProblem> Validate(ZoneContent zone)
    {
        var problems = new List<ContentProblem>();
        Check(zone.ZoneId, ContentKinds.Deployables, zone.Deployables, problems, d => d.Id);
        Check(zone.ZoneId, ContentKinds.Outposts, zone.Outposts, problems, null);
        Check(zone.ZoneId, ContentKinds.Meldings, zone.Meldings, problems, m => m.Id);
        Check(zone.ZoneId, ContentKinds.Encounters, zone.Encounters, problems, e => e.Id);

        foreach (var melding in zone.Meldings.Where(m => m.ControlPoints.Count != m.Offsets.Count))
        {
            problems.Add(new ContentProblem(zone.ZoneId, ContentKinds.Meldings.Name, melding.Key, $"{melding.ControlPoints.Count} control points but {melding.Offsets.Count} offsets"));
        }

        foreach (var deployable in zone.Deployables.Where(d => d.Orientation.LengthSquared() < 1e-6f))
        {
            problems.Add(new ContentProblem(zone.ZoneId, ContentKinds.Deployables.Name, deployable.Key, "orientation is all zero"));
        }

        return problems;
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
