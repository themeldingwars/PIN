namespace GameServer.StaticDB.Records.dbitems;
public record class ItemCertificateRequirements
{
    public uint ItemId { get; set; }
    public ushort RequiredCertificate { get; set; }
}