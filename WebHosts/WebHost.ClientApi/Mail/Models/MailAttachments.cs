namespace WebHost.ClientApi.Mail.Models;

public class MailAttachments
{
    public uint ItemSdbId { get; set; }
    public ulong? ItemId { get; set; }
    public uint Quantity { get; set; }
    public bool Claimed { get; set; }
}