namespace GameServer.Data.SDB.Records.apt;
public record class WhileLoopCommandDef
{
    public uint BodyChain { get; set; }
    public uint ConditionChain { get; set; }
    public uint Id { get; set; }
    public byte DoWhile { get; set; }
}