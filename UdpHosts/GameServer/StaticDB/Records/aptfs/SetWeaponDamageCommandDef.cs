namespace GameServer.Data.SDB.Records.aptfs;
public record class SetWeaponDamageCommandDef
{
    public float Dmgmaxvalue { get; set; }
    public float Lerpminvalue { get; set; }
    public float Lerpmaxvalue { get; set; }
    public float Dmgminvalue { get; set; }
    public uint Id { get; set; }
    public byte Set { get; set; }
    public byte Clamplerp { get; set; }
    public byte DamageRegop { get; set; }
    public byte Lerpfallheight { get; set; }
    public byte Multiply { get; set; }
    public byte Lerpenergy { get; set; }
}
