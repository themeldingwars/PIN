using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameServer.StaticDB;
using Serilog;
using Shared.Content;
using Shared.Content.Zones;
using Shared.Content.Zones.Encounters;

namespace GameServer.Systems.Content;

/// <summary>The zones' content from the content folder, see its README. A zone without a folder has no content.</summary>
public sealed class ContentStore
{
    private readonly ConcurrentDictionary<uint, ZoneContent> _zones = new();
    private readonly ILogger _logger;

    public ContentStore(string root, ILogger logger)
    {
        Root = Path.GetFullPath(root, AppContext.BaseDirectory);
        _logger = logger.ForContext<ContentStore>();
    }

    public string Root { get; }

    /// <summary>Loads every zone. Needs the SDB, the content refers to it.</summary>
    /// <exception cref="InvalidDataException">A file can't be read or the content has problems, the log lists them.</exception>
    public void Load()
    {
        _logger.Information("Reading content from {ContentRoot}", Root);

        var failed = false;
        foreach (var zoneId in ContentFolder.Zones(Root))
        {
            if (TryLoadZone(zoneId, out var zone))
            {
                _zones[zoneId] = zone;
            }
            else
            {
                failed = true;
            }
        }

        if (failed)
        {
            throw new InvalidDataException($"The content in {Root} has problems, see the log");
        }

        _logger.Information(
            "Read content of {ZoneCount} zones: {Deployables} deployables, {Outposts} outposts, {Meldings} meldings, {Encounters} encounters",
            _zones.Count,
            _zones.Values.Sum(z => z.Deployables.Count),
            _zones.Values.Sum(z => z.Outposts.Count),
            _zones.Values.Sum(z => z.Meldings.Count),
            _zones.Values.Sum(z => z.Encounters.Count));
    }

    public ZoneContent Zone(uint zoneId) => _zones.TryGetValue(zoneId, out var zone) ? zone : new ZoneContent(zoneId);

    private static IEnumerable<ContentProblem> CheckSdb(ZoneContent zone)
    {
        foreach (var deployable in zone.Deployables.Where(d => SDBInterface.GetDeployable(d.Type) == null))
        {
            yield return new ContentProblem(zone.ZoneId, ContentKinds.Deployables.Name, deployable.Key, $"deployable type {deployable.Type} isn't in the SDB");
        }

        foreach (var repulsor in zone.Encounters.OfType<ZoneMeldingRepulsor>())
        {
            foreach (var placed in new[] { repulsor.Repulsor, repulsor.Terminal }.Where(p => SDBInterface.GetDeployable(p.Type) == null))
            {
                yield return new ContentProblem(zone.ZoneId, ContentKinds.Encounters.Name, repulsor.Key, $"deployable type {placed.Type} isn't in the SDB");
            }
        }
    }

    private bool TryLoadZone(uint zoneId, out ZoneContent zone)
    {
        try
        {
            zone = ContentFolder.LoadZone(Root, zoneId);
        }
        catch (Exception e) when (e is ContentException or IOException)
        {
            _logger.Error("Can't read the content of zone {ZoneId}: {Error}", zoneId, e.Message);
            zone = null;
            return false;
        }

        var problems = ContentValidator.Validate(zone).Concat(CheckSdb(zone)).ToList();
        foreach (var problem in problems)
        {
            _logger.Error("Content problem in {Problem}", problem);
        }

        return problems.Count == 0;
    }
}
