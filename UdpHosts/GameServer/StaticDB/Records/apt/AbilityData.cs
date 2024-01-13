namespace GameServer.Data.SDB.Records.apt;
public record class AbilityData
{
    public uint LocalizedNameId { get; set; }
    public uint Chain { get; set; }
    public uint OrnamentsMapGroupId { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public uint AbilityIcon { get; set; }
    public uint Id { get; set; }
}