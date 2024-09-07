namespace GameServer.Data.SDB.Records.apt;
public record class TargetPreviousCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Clearformer { get; set; }
}