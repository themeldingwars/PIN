namespace GameServer.Data.SDB.Records.apt;
public record class NamedVarConstants
{
    public uint Id { get; set; }
    public uint AbilityId { get; set; }
    public float NamedvarValue { get; set; }
    public ushort NamedvarType { get; set; }

}