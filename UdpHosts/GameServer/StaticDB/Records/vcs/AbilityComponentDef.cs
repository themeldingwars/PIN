namespace GameServer.Data.SDB.Records.vcs;
public record class AbilityComponentDef
{
    public uint AbilityId { get; set; }
    public uint Id { get; set; }
    public byte AbilityType { get; set; }
}