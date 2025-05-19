namespace GameServer.Data.SDB.Records.customdata;

public record UnpackItemCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint PackageSdbId { get; set; } = 0;
    public uint ItemSdbId { get; set; } = 0;
}