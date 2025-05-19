namespace GameServer.Data.SDB.Records.vcs;
public record class CargoComponentDef
{
    public float TransferRate { get; set; }
    public uint AssetId { get; set; }
    public uint Capacity { get; set; }
    public string Hardpoint { get; set; }
    public uint ResourceTypeId { get; set; }
    public uint Id { get; set; }
}