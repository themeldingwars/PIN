namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireLevelCommandDef
{
    public uint Id { get; set; }
    public ushort Level { get; set; }
    public byte FrameLevel { get; set; }
    public byte SessionLevel { get; set; }
}
