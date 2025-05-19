namespace GameServer.Data.SDB.Records.aptfs;
public record class AttemptToCalldownVehicleCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort VehicleType { get; set; }
}