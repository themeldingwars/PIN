namespace GameServer.Data.SDB.Records.dbitems;
public record class Battleframe
{
    public uint ProgressionTitleId { get; set; }
    public float FastSpeed { get; set; }
    public float RunspeedMult { get; set; }
    public float NormalSpeed { get; set; }
    public uint GibsetId { get; set; }
    public float CrouchSpeedMult { get; set; }
    public float BodyRadius { get; set; }
    public int ShieldRechargePerSec { get; set; }
    public float XprewardMult { get; set; }
    public uint DefaultFullbodyPaletteId { get; set; }
    public float ReverseSpeedMult { get; set; }
    public uint AbilityGroupId { get; set; }
    public int BaseShields { get; set; }
    public float MaxBuffSpeed { get; set; }
    public uint DefaultGlowPaletteId { get; set; }
    public uint PosetypeId { get; set; }
    public uint EnergyRechargeDelayMs { get; set; }
    public uint ScalingTableId { get; set; }
    public float MinRandScale { get; set; }
    public float EnergyRechargePerSec { get; set; }
    public float StrafeSpeedMult { get; set; }
    public float BodyMass { get; set; }
    public uint XprewardType { get; set; }
    public float RotationSpeed { get; set; }
    public int BaseHealth { get; set; }
    public float BaseEnergy { get; set; }
    public uint DefaultBodysuitPaletteId { get; set; }
    public uint ShieldRechargeDelayMs { get; set; }
    public uint VisualGroup { get; set; }
    public uint PvpFrameItemId { get; set; }
    public uint DefaultArmorPaletteId { get; set; }
    public float MaxRandScale { get; set; }
    public uint CoreAbilityGroupId { get; set; }
    public float JumpHeight { get; set; }
    public uint Id { get; set; }
    public float BodyHeight { get; set; }
    public short MaxProgressionLevel { get; set; }
    public short MinProgressionLevel { get; set; }
    public byte AnimArmedId { get; set; }
    public byte Archtype { get; set; }
    public byte AnimArmedPriority { get; set; }
    public byte DamageResponse { get; set; }
}
