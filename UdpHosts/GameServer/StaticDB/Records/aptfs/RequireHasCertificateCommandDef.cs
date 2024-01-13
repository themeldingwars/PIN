namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireHasCertificateCommandDef
{
    public uint CertificateId { get; set; }
    public uint Id { get; set; }
    public byte Negate { get; set; }
}
