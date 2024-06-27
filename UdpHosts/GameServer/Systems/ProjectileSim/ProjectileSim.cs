using System.Numerics;
using System.Threading;
using GameServer.Data.SDB.Records.dbitems;
using GameServer.Entities.Character;

namespace GameServer;

public class ProjectileSim
{
    private Shard _shard;

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