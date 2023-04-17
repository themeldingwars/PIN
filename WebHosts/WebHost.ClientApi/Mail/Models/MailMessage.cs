using System;

namespace WebHost.ClientApi.Mail.Models;

public class MailMessage
{
    public uint Id { get; set; }
    public string Subject { get; set; }
    public ulong SenderGuid { get; set; }
    public string SenderName { get; set; }
    public string Body { get; set; }
    public bool Unread { get; set; }
    public string MailType { get; set; }
    public uint CreatedAt { get; set; }
    public uint AttachmentCount { get; set; }
    public Array Attachments { get; set; }
}