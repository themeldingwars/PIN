namespace GameServer.Data.SDB.Records.apt;
public record class LogicNegateCommandDef : ICommandDef
{
    public uint NegateChain { get; set; }
    public uint Id { get; set; }
}