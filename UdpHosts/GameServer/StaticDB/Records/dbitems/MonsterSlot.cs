namespace GameServer.StaticDB.Records.dbitems;
public record class MonsterSlot
{
    public uint MonsterId { get; set; }
    public uint DefaultAbility { get; set; }
}