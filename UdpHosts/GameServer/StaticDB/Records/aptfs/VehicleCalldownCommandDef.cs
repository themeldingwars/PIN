namespace GameServer.Data.SDB.Records.aptfs;
public record class VehicleCalldownCommandDef : ICommandDef
{
    public float OutdoorsHeight { get; set; }
    public uint Id { get; set; }
    public ushort VehicleType { get; set; }
}