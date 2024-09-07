namespace GameServer.Data.SDB.Records.aptfs;
public record class FireProjectileCommandDef : ICommandDef
{
    public string AimOffset { get; set; }
    public uint Ammotype { get; set; }
    public float Range { get; set; }
    public float Damage { get; set; }
    public float Spread { get; set; }
    public string AimOriginOffset { get; set; }
    public uint Id { get; set; }
    public ushort Hardpoint { get; set; }
    public byte BodyOrigin { get; set; }
    public byte RangeRegop { get; set; }
    public byte DamageRegop { get; set; }
    public byte UseWeaponDamage { get; set; }
    public byte Burstcount { get; set; }
    public byte AimWithGravity { get; set; }
    public byte UseHomingTarget { get; set; }
    public byte AbsoluteOffset { get; set; }
    public byte ImpactOrient { get; set; }
    public byte PassBonus { get; set; }
    public byte AimAtTarget { get; set; }
}
