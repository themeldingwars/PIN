namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemTypeAttributeModifier
{
    public uint AttributeId { get; set; }
    public float WeightCoefficient { get; set; }
    public float PowerCoefficient { get; set; }
    public float CpuCoefficient { get; set; }
    public uint MapToAttributeId { get; set; }
    public float MaxFloat { get; set; }
    public uint CraftingTypeId { get; set; }
    public float MinFloat { get; set; }
    public byte Optional { get; set; }
    public byte StageModifiable { get; set; }
}