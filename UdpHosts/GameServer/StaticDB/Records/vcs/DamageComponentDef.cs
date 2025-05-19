namespace GameServer.Data.SDB.Records.vcs;
public record class DamageComponentDef
{
    public float MinSpeedToDamageNpc { get; set; }
    public float MaxScalableHitPoints { get; set; }
    public float CollisionDamageScalar { get; set; }
    public float MeldingDamageScalar { get; set; }
    public float NpcDamageScalar { get; set; }
    public float MinSpeedCollisionDamage { get; set; }
    public uint DamageResponse { get; set; }
    public float NpcImpulseStrength { get; set; }
    public float MaxHitPoints { get; set; }
    public uint Id { get; set; }
    public ushort DeathAbility { get; set; }
    public ushort LowHealthDamageEffect { get; set; }
}