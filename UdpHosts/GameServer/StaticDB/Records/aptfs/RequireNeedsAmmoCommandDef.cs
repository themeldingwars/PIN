namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireNeedsAmmoCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte CheckSecondary { get; set; }
    public byte Negate { get; set; }
    public byte CheckPrimary { get; set; }
}