namespace GameServer.Data.SDB.Records.dbcharacter;
public record class MailMessages
{
    public uint Id { get; set; }
    public string MailType { get; set; }
    public uint LocSubjectId { get; set; }
    public uint LocBodyId { get; set; }
    public uint SenderNpc { get; set; }
}