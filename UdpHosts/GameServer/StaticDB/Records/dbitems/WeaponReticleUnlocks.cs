namespace GameServer.Data.SDB.Records.dbitems;
public record class WeaponReticleUnlocks
{
    public string Name { get; set; }
    public uint Id { get; set; }
    public byte AlwaysUnlocked { get; set; }
}
