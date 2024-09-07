namespace GameServer.Data.SDB.Records.apt;
public record class TargetDifferenceCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte ReplaceFormer { get; set; }
    public byte SwapCurrentFormer { get; set; }
}