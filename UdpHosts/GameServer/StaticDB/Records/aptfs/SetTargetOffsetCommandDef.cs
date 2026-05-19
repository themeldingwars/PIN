namespace GameServer.StaticDB.Records.aptfs;
public record class SetTargetOffsetCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Aimpos { get; set; }
    public byte Initiation { get; set; }
}