namespace GameServer.Data.SDB.Records.dbitems;
public record class VendorTokenMachine
{
    public uint NameId { get; set; }
    public uint DescriptionId { get; set; }
    public uint WebVendorId { get; set; }
    public uint JackpotStatusfxId { get; set; }
    public uint RollStatusfxId { get; set; }
    public uint WinStatusfxId { get; set; }
    public uint Id { get; set; }
}
