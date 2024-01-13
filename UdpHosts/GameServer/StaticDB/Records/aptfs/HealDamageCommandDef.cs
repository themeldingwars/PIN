namespace GameServer.Data.SDB.Records.aptfs;
public record class HealDamageCommandDef
{
    public int Healpoints { get; set; }
    public uint Id { get; set; }
    public byte HealpointsRegop { get; set; }
    public byte Usedmgdealt { get; set; }
    public byte DamageType { get; set; }
    public byte Weapondamage { get; set; }
}
