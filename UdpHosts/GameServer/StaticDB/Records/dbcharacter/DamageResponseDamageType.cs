namespace GameServer.Data.SDB.Records.dbcharacter;
public record class DamageResponseDamageType
{
    public float Multiplier { get; set; }
    public uint Id { get; set; }
    public byte Damageresponse { get; set; }
    public byte Damagetype { get; set; }
}
