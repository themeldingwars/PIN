namespace GameServer.Data.SDB.Records.apt;
public record class TargetSwapCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte ClearFormer { get; set; }
    public byte ClearCurrent { get; set; }
}