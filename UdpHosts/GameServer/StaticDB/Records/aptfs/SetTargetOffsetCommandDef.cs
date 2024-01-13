namespace GameServer.Data.SDB.Records.aptfs;
public record class SetTargetOffsetCommandDef
{
    public uint Id { get; set; }
    public byte Aimpos { get; set; }
    public byte Initiation { get; set; }
}
