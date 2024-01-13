namespace GameServer.Data.SDB.Records.dbcharacter;
public record class FactionReputations
{
    public uint NameId { get; set; }
    public uint FactionId { get; set; }
    public int MinReputation { get; set; }
    public sbyte HostilityStance { get; set; }
}
