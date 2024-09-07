namespace GameServer.Data.SDB.Records.apt;
public record class LogicOrChainCommandDef : ICommandDef
{
    public uint OrChain { get; set; }
    public uint Id { get; set; }
    public byte AlwaysSuccess { get; set; }
}