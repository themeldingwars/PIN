using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Shared.Collision.Navigation;

namespace GameServer.Navigation;

/// <summary>
///     A navmesh and crowd per baked agent size. Agents only avoid the others on their own layer, a Brontodon and a
///     human still bump into each other through physics.
/// </summary>
public sealed class ShardNavigation
{
    public ShardNavigation(uint zoneId, GameServerSettings settings, ILogger logger)
    {
        Layers = NavMeshBuildParams.All
            .Select(agent =>
            {
                var mesh = new NavMeshService(logger);
                mesh.Load(zoneId, settings.CachePath, agent);
                return new NavigationLayer(agent, mesh, new CrowdNavigationService(mesh, agent, settings));
            })
            .ToArray();
    }

    public IReadOnlyList<NavigationLayer> Layers { get; }

    /// <summary>The mesh for human-sized agents, also used for anything that isn't about one NPC's body.</summary>
    public NavigationLayer Default => Layers[0];

    /// <summary>The loaded layer whose agent radius is closest to the body's, the default when no other is loaded.</summary>
    public NavigationLayer For(float bodyRadius)
    {
        var best = Default;
        foreach (var layer in Layers)
        {
            if (layer.Mesh.IsLoaded && MathF.Abs(layer.Agent.AgentRadius - bodyRadius) < MathF.Abs(best.Agent.AgentRadius - bodyRadius))
            {
                best = layer;
            }
        }

        return best;
    }

    /// <summary>Steps every crowd once, after dropping the agents of NPCs that are gone.</summary>
    public void Update(float dt, IReadOnlySet<ulong> liveEntityIds)
    {
        foreach (var layer in Layers)
        {
            layer.Crowd.RemoveStaleAgents(liveEntityIds);
            layer.Crowd.Update(dt);
        }
    }
}
