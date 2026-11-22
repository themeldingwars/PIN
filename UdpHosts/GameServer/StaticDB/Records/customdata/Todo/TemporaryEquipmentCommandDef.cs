namespace GameServer.StaticDB.Records.customdata;

public record TemporaryEquipmentCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint DurationMs { get; set; }
    public uint PrimaryItemId { get; set; }
    public uint SecondaryItemId { get; set; }
}