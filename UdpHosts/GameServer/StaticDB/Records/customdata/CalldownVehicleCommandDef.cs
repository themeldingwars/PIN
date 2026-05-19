namespace GameServer.StaticDB.Records.customdata;

public record CalldownVehicleCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort VehicleId { get; set; } = 0;
}