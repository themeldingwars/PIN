namespace GameServer.StaticDB.Records.aptfs;
public record class RequireWeaponArmedCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte WeaponIndex { get; set; }
    public byte Negate { get; set; }
}