namespace GameServer.Data.SDB.Records.vcs;
public record class VehicleInfo
{
    public uint LocalizedNameId { get; set; }
    public uint ScalingTableId { get; set; }
    public uint FactionId { get; set; }
    public int GameplayPriorityBoost { get; set; }
    public ushort Id { get; set; }
    public byte VehicleClass { get; set; }
    public byte Race { get; set; }
}