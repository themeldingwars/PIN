namespace GameServer.Data.SDB.Records.apt;
public record class LogicAndChainCommandDef : ICommandDef
{
    public uint AndChain { get; set; }
    public uint Id { get; set; }
    public byte AlwaysSuccess { get; set; }
}