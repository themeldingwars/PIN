namespace GameServer.Data.SDB.Records.aptfs;
public record class ActivationDurationCommandDef : ICommandDef
{
    public uint AbilityId { get; set; }
    public uint Id { get; set; }
    public byte Activated { get; set; }
}