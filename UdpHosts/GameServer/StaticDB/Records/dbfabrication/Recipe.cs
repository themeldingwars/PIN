namespace GameServer.Data.SDB.Records.dbfabrication;
public record class Recipe
{
    public uint LocalizedNameId { get; set; }
    public uint BuildMax { get; set; }
    public uint ItemType { get; set; }
    public uint BuildCost { get; set; }
    public uint PostActionGroupId { get; set; }
    public uint TinkerCriticalSuccessId { get; set; }
    public uint ActionGroupId { get; set; }
    public uint Certificate { get; set; }
    public float QualityScale { get; set; }
    public uint BaseQuantity { get; set; }
    public float BaseBuildTime { get; set; }
    public uint PreActionGroupId { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public uint ResultMinQuality { get; set; }
    public float QualityScaleAutogen { get; set; }
    public uint TinkerRarityLevelsId { get; set; }
    public uint QualityBase { get; set; }
    public uint ResultMaxQuality { get; set; }
    public uint BaseActions { get; set; }
    public uint ResultLootTableId { get; set; }
    public uint Id { get; set; }
    public uint BaseLevel { get; set; }
    public uint QualityBaseAutogen { get; set; }
}