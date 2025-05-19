namespace GameServer.Data.SDB.Records.dbitems;
public record class ItemCertificateRequirements
{
    public uint ItemId { get; set; }
    public ushort RequiredCertificate { get; set; }
}