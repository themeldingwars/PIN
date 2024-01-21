namespace GameServer.Data.SDB.Records.apt;
public record class ConditionalBranchCommandDef
{
    public uint IfChain { get; set; }
    public uint ThenChain { get; set; }
    public uint ElseChain { get; set; }
    public uint Id { get; set; }
}