namespace GameServer.Data.SDB.Records.dbcharacter;
public record class CharInfo
{
    // public Vec3 ProjectileOffset { get; set; }
    public float ScopeRange { get; set; }
    public uint DeathparticleId { get; set; }
    public uint AoeHitparticleId { get; set; }
    public float GroundAlignment { get; set; }
    public int GameplayPriorityBoost { get; set; }
    public uint HitparticleId { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public byte RequiresRagdoll { get; set; }
    public byte IsNpc { get; set; }
    public byte IsHumanoid { get; set; }
}
