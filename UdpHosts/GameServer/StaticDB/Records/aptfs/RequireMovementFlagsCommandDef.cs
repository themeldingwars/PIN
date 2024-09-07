namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireMovementFlagsCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Crouch { get; set; }
    public byte Sprint { get; set; }
    public byte Negate { get; set; }
}
