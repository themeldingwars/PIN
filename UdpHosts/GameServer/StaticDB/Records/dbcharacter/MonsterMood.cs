namespace GameServer.Data.SDB.Records.dbcharacter;
public record class MonsterMood
{
    public uint MonsterId { get; set; }
    public uint Mood { get; set; }
    public uint PortraitId { get; set; }
}
