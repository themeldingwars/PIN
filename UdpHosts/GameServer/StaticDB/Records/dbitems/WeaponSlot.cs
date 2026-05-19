namespace GameServer.StaticDB.Records.dbitems;
public record class WeaponSlot
{
    public uint WeaponId { get; set; }
    public uint DefaultAbility { get; set; }
}