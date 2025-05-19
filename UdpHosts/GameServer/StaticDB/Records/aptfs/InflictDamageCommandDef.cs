namespace GameServer.Data.SDB.Records.aptfs;
public record class InflictDamageCommandDef : ICommandDef
{
    public float Hitshakemult { get; set; }
    public int Damagepoints { get; set; }
    public float Splashrange { get; set; }
    public float Pointblankrange { get; set; }
    public uint Id { get; set; }
    public byte SplashrangeRegop { get; set; }
    public byte DamagepointsRegop { get; set; }
    public byte UseWeaponRadius { get; set; }
    public byte Weapondamagetype { get; set; }
    public byte Usedmgdealt { get; set; }
    public byte Falloff { get; set; }
    public byte DamageType { get; set; }
    public byte Weapondamage { get; set; }
    public byte Frominitiatorpos { get; set; }
}