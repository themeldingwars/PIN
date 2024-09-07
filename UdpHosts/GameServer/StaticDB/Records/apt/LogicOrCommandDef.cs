namespace GameServer.Data.SDB.Records.apt;
public record class LogicOrCommandDef : ICommandDef
{
    public uint BChain { get; set; }
    public uint AChain { get; set; }
    public uint Id { get; set; }
}