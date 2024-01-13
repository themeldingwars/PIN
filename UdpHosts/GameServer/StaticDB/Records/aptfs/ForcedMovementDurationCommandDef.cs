namespace GameServer.Data.SDB.Records.aptfs;
public record class ForcedMovementDurationCommandDef
{
    public uint Id { get; set; }
    public byte Negate { get; set; }
}
