namespace GameServer.Data.SDB.Records.dbcharacter;
public record class MonsterScaling
{
    public uint Damage { get; set; }
    public uint Health { get; set; }
    public byte Level { get; set; }
}