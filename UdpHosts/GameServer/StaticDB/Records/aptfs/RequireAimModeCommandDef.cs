namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireAimModeCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Negate { get; set; }
}
