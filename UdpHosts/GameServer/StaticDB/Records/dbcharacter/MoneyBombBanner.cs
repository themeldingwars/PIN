namespace GameServer.Data.SDB.Records.dbcharacter;
public record class MoneyBombBanner
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public uint SponsorId { get; set; }
    public uint BrandId { get; set; }
    public uint WebIconId { get; set; }
    public uint TitleId { get; set; }
}