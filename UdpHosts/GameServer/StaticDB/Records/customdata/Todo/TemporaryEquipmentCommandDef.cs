namespace GameServer.Data.SDB.Records.customdata;

public record TemporaryEquipmentCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint DurationMs { get; set; }
}
