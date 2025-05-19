namespace GameServer.Data.SDB.Records.vcs;
public record class WheelUpgradeComponentDef
{
    public uint WheelAssetId1 { get; set; }
    public uint WheelAssetId2 { get; set; }
    public uint WheelAssetId3 { get; set; }
    public uint Id { get; set; }
}