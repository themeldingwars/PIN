namespace GameServer.Data.SDB.Records.apt;
public record class LogicAndChainCommandDef
{
    public uint AndChain { get; set; }
    public uint Id { get; set; }
    public byte AlwaysSuccess { get; set; }
}