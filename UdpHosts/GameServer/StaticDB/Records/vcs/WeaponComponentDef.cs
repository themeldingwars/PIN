namespace GameServer.Data.SDB.Records.vcs;
public record class WeaponComponentDef
{
    public uint VisualrecId { get; set; }
    public uint AnimnetId { get; set; }
    public string Hardpoint { get; set; }
    public uint TurretType { get; set; }
    public uint Id { get; set; }
}
