namespace GameServer.Data.SDB.Records.apt;
public record class ReturnCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort ReturnStatus { get; set; }
    public byte ReturnSuccess { get; set; }
    public byte ReturnYield { get; set; }
    public byte ReturnHalt { get; set; }
}