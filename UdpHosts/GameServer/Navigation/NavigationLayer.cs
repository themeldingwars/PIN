using Shared.Collision.Navigation;

namespace GameServer.Navigation;

public sealed record NavigationLayer(NavMeshBuildParams Agent, NavMeshService Mesh, CrowdNavigationService Crowd);
