namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireMovestateCommandDef
{
    public uint Id { get; set; }
    public byte Falling { get; set; }
    public byte Gliding { get; set; }
    public byte Jetpack { get; set; }
    public byte Sliding { get; set; }
    public byte Walking { get; set; }
    public byte Stall { get; set; }
    public byte Thruster { get; set; }
    public byte Running { get; set; }
    public byte KnockdownFalling { get; set; }
    public byte JetpackSprint { get; set; }
    public byte KnockdownOnground { get; set; }
    public byte Standing { get; set; }
    public byte Negate { get; set; }
}
