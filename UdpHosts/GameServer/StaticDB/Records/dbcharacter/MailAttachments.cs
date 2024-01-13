namespace GameServer.Data.SDB.Records.dbcharacter;
public record class MailAttachments
{
    public ushort Quantity { get; set; }
    public uint Id { get; set; }
    public uint Order { get; set; }
    public uint MailMsgId { get; set; }
    public uint ItemId { get; set; }
}
