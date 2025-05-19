namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireIsNPCCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Negate { get; set; }
}