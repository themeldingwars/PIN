namespace GameServer.Data.SDB.Records.dbitems;
public record class WeaponSpecialAmmo
{
    public uint Id { get; set; }
    public uint Weapontype { get; set; }
    public ushort AmmoId { get; set; }
    public byte BuiltIn { get; set; }
}