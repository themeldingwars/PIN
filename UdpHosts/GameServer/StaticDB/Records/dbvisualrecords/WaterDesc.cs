namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class WaterDesc
{
    public uint IdlePfxId { get; set; }
    public uint MovingRestrictedVehicleStatusEffectId { get; set; }
    public float DrowningPercent { get; set; }
    public uint EnterHardPfxId { get; set; }
    public uint OceanShaderId { get; set; }
    public uint DrowningCharStatusEffectId { get; set; }
    public uint MovingUnrestrictedPfxId { get; set; }
    public uint DrowningVehicleStatusEffectId { get; set; }
    public uint PhysicsMaterialId { get; set; }
    public uint LakeShaderId { get; set; }
    public uint RiverShaderId { get; set; }
    public uint EnterSoftPfxId { get; set; }
    public uint MovingRestrictedPfxId { get; set; }
    public string LakeShader { get; set; }
    public uint DyingCharStatusEffectId { get; set; }
    public string RiverShader { get; set; }
    public float MovingRestrictedPercent { get; set; }
    public uint DyingVehicleStatusEffectId { get; set; }
    public uint MovingRestrictedCharStatusEffectId { get; set; }
    public uint DrowningPfxId { get; set; }
    public uint Id { get; set; }
    public float DyingPercent { get; set; }
    public string OceanShader { get; set; }
    public byte DefinesPhysicsProperties { get; set; }
}
