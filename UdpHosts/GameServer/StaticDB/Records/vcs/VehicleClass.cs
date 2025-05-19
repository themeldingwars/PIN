namespace GameServer.Data.SDB.Records.vcs;
public record class VehicleClass
{
    public byte PerformanceSlots { get; set; }
    public byte DefenseSlots { get; set; }
    public byte HandlingSlots { get; set; }
    public byte OffenseSlots { get; set; }
    public string Name { get; set; }
    public byte Id { get; set; }
}