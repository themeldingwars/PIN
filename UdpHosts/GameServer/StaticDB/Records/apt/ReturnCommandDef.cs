namespace GameServer.Data.SDB.Records.apt;
public record class ReturnCommandDef
{
    public uint Id { get; set; }
    public ushort ReturnStatus { get; set; }
    public byte ReturnSuccess { get; set; }
    public byte ReturnYield { get; set; }
    public byte ReturnHalt { get; set; }
}