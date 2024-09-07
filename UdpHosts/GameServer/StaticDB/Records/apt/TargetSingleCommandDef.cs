namespace GameServer.Data.SDB.Records.apt;
public record class TargetSingleCommandDef : ICommandDef
{
    public float Range { get; set; }
    public uint Id { get; set; }
    public byte SetOffset { get; set; }
    public byte UseInitPos { get; set; }
    public byte IgnoreWalls { get; set; }
    public byte StaticOnly { get; set; }
}