namespace GameServer.Data.SDB.Records.aptfs;
public record class RequirePermissionCommandDef
{
    public uint Id { get; set; }
    public byte Permission { get; set; }
    public byte Negate { get; set; }
}
