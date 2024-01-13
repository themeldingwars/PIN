namespace GameServer.Data.SDB.Records.dbcharacter;
public record class LoadoutRareReplacements
{
    public uint Level { get; set; }
    public uint ChassisId { get; set; }
    public uint ReplacesId { get; set; }
    public uint Random { get; set; }
    public uint ItemId { get; set; }
}
