namespace GameServer.StaticDB.Records.aptfs;
public record class ForcedMovementDurationCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Negate { get; set; }
}