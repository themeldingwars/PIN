using System.Numerics;
using GameServer.Data.SDB.Records.dbitems;
using GameServer.Entities.Character;
using Serilog;

namespace GameServer;

public class ProjectileSim
{
    private readonly IShard _shard;
    private readonly ILogger _logger;

    public ProjectileSim(IShard shard, ILogger logger)
    {
        _shard = shard;
        _logger = logger;
    }

    public void FireProjectile(CharacterEntity entity, uint trace, Vector3 origin, Vector3 direction, Ammo ammo)
    {
        _shard.Physics.ProjectileRayCast(origin, direction, entity, trace);
    }

    /*
    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
    }
    */
}