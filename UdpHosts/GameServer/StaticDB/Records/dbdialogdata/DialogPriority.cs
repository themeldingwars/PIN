namespace GameServer.Data.SDB.Records.dbdialogdata;
public record class DialogPriority
{
    public uint Level { get; set; }
    public uint Id { get; set; }
    public byte SinImprint { get; set; }
    public byte Interrupts { get; set; }
}