namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class ForceShieldType
{
    public uint Flags { get; set; }
    public uint MaterialId { get; set; }
    public uint PfxId { get; set; }
    public uint Id { get; set; }
}