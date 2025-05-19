namespace GameServer.Data.SDB.Records.dbitems;
public record class WeaponParticleFX
{
    public uint MuzzleDischarge { get; set; }
    public uint StaticEffects { get; set; }
    public uint MuzzleBurst { get; set; }
    public uint WeapontypeId { get; set; }
    public uint BuiltinStaticEffects { get; set; }
    public uint ChargeEnd { get; set; }
    public uint MuzzleLocked { get; set; }
    public uint ProjParabolaEffect { get; set; }
    public uint ChargeStart { get; set; }
    public uint LaserSightEffect { get; set; }
    public uint FlashlightEffect { get; set; }
    public uint WeaponId { get; set; }
    public uint ShellEnd { get; set; }
    public uint MuzzleStart { get; set; }
    public uint ChargeOver { get; set; }
    public uint MuzzleEnd { get; set; }
    public uint AoeParabolaEffect { get; set; }
    public uint ShellDischarge { get; set; }
    public uint ShellStart { get; set; }
    public uint Id { get; set; }
    public uint AmmotypeId { get; set; }
}