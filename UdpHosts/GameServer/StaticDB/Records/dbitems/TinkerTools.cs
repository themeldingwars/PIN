namespace GameServer.Data.SDB.Records.dbitems;
public record class TinkerTools
{
    public float BonusModifier { get; set; }
    public uint Id { get; set; }
    public short MaxRank { get; set; }
    public short MinRank { get; set; }
    public byte ApprovalState { get; set; }
}