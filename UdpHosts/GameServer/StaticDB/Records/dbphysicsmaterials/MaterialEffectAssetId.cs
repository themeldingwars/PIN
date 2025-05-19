namespace GameServer.Data.SDB.Records.dbphysicsmaterials;
public record class MaterialEffectAssetId
{
    public uint AudioVisualMaterialId { get; set; }
    public uint MaterialEffectId { get; set; }
    public uint AssetId { get; set; }
    public uint Id { get; set; }
}