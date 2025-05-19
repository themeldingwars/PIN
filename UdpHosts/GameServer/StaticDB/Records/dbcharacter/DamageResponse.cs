namespace GameServer.Data.SDB.Records.dbcharacter;
public record class DamageResponse
{
    public float DefaultMultiplier { get; set; }
    public string Name { get; set; }
    public byte Id { get; set; }
}