namespace GameServer.StaticDB.Records.aptfs;
public record class TargetSquadmatesCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte FailNone { get; set; }
    public byte Filter { get; set; }
}