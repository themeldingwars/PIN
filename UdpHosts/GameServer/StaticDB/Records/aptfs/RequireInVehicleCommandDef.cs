namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireInVehicleCommandDef
{
    public uint Id { get; set; }
    public byte Passenger { get; set; }
    public byte Driver { get; set; }
    public byte Negate { get; set; }
}
