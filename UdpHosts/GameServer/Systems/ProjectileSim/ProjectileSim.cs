using System.Numerics;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.dbitems;

namespace GameServer.Systems.ProjectileSim;

public class ProjectileSim
{
    private readonly Shard _shard;

    public ProjectileSim(Shard shard)
    {
        _shard = shard;
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