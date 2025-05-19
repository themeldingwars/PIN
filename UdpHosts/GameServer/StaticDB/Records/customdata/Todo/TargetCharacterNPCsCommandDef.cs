namespace GameServer.Data.SDB.Records.customdata;

public record TargetCharacterNPCsCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte FailNone { get; set; }
    public byte Filter { get; set; }
}