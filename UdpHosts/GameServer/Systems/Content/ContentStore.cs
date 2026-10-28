using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GameServer.StaticDB;
using Serilog;
using Shared.Content;
using Shared.Content.Zones;
using Shared.Content.Zones.Encounters;

namespace GameServer.Systems.Content;

/// <summary>The zones' content from the content folder, see its README. A zone without a folder has no content.</summary>
public sealed class ContentStore
{
    private static readonly TimeSpan _reloadDelay = TimeSpan.FromMilliseconds(300);

    private readonly ConcurrentDictionary<uint, ZoneContent> _zones = new();
    private readonly ConcurrentDictionary<uint, Timer> _pendingReloads = new();
    private readonly ILogger _logger;
    private FileSystemWatcher _watcher;

    public ContentStore(string root, ILogger logger)
    {
        Root = Path.GetFullPath(root, AppContext.BaseDirectory);
        _logger = logger.ForContext<ContentStore>();
    }

    /// <summary>A zone's content was reloaded, with the content before and after. Raised on a thread pool thread.</summary>
    public event Action<ZoneContent, ZoneContent> ZoneChanged;

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

    /// <summary>
    ///     Reloads a zone whenever one of its files changes. Content with problems is logged and the zone keeps what it had.
    /// </summary>
    public void Watch()
    {
        Directory.CreateDirectory(Path.Combine(Root, "zones"));
        _watcher = new FileSystemWatcher(Root, "*.json") { IncludeSubdirectories = true };
        _watcher.Changed += (_, e) => ScheduleReload(e.FullPath);
        _watcher.Created += (_, e) => ScheduleReload(e.FullPath);
        _watcher.Deleted += (_, e) => ScheduleReload(e.FullPath);
        _watcher.Renamed += (_, e) =>
        {
            ScheduleReload(e.OldFullPath);
            ScheduleReload(e.FullPath);
        };
        _watcher.EnableRaisingEvents = true;

        _logger.Information("Watching {ContentRoot} for changes", Root);
    }

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

    // Editors save in several steps (temporary file, rename, attributes), the reload waits until they're done
    private void ScheduleReload(string path)
    {
        if (!ContentFolder.TryGetZone(Root, path, out var zoneId))
        {
            return;
        }

        _pendingReloads.AddOrUpdate(
            zoneId,
            id => new Timer(_ => Reload(id), null, _reloadDelay, Timeout.InfiniteTimeSpan),
            (_, timer) =>
            {
                timer.Change(_reloadDelay, Timeout.InfiniteTimeSpan);
                return timer;
            });
    }

    private void Reload(uint zoneId)
    {
        if (_pendingReloads.TryRemove(zoneId, out var timer))
        {
            timer.Dispose();
        }

        if (!TryLoadZone(zoneId, out var after))
        {
            _logger.Warning("Keeping the previous content of zone {ZoneId}", zoneId);
            return;
        }

        var before = Zone(zoneId);
        _zones[zoneId] = after;
        _logger.Information("Reloaded the content of zone {ZoneId}", zoneId);
        ZoneChanged?.Invoke(before, after);
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
