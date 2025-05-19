namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireWeaponTemplateCommandDef : ICommandDef
{
    public uint WeaponTemplateId { get; set; }
    public uint Id { get; set; }
    public byte Negate { get; set; }
}