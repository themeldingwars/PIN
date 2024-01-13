namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireMovementFlagsCommandDef
{
    public uint Id { get; set; }
    public byte Crouch { get; set; }
    public byte Sprint { get; set; }
    public byte Negate { get; set; }
}
