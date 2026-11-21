namespace GameServer.Systems.ProjectileSim;

public enum ProjectileHitReason : byte
{
    None = 0,
    Impact = 1,
    Touch = 2,
    Airburst = 3,
    Period = 4,
}
