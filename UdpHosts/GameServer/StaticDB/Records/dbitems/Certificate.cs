namespace GameServer.Data.SDB.Records.dbitems;
public record class Certificate
{
    public uint LocalizedNameId { get; set; }
    public uint XpValue { get; set; }
    public uint WebIconId { get; set; }
    public uint RewardZoneId { get; set; }
    public uint BonusAmount { get; set; }
    public uint XpType { get; set; }
    public uint LocalizedDescription { get; set; }
    public uint CertType { get; set; }
    public uint Id { get; set; }
    public byte RequirementsOp { get; set; }
}
