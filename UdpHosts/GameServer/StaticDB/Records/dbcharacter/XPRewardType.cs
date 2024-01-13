namespace GameServer.Data.SDB.Records.dbcharacter;
public record class XPRewardType
{
    public uint BaseXp { get; set; }
    public string UiKey { get; set; }
    public uint Id { get; set; }
    public byte SquadSharable { get; set; }
}
