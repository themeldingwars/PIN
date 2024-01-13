namespace GameServer.Data.SDB.Records.aptfs;
public record class RegisterLoadFromWeaponCommandDef
{
    public uint Id { get; set; }
    public byte WeaponStat { get; set; }
    public byte Regop { get; set; }
}
