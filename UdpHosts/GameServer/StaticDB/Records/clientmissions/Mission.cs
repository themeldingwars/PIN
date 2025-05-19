namespace GameServer.Data.SDB.Records.clientmissions;
public record class Mission
{
    public uint LocalizedNameId { get; set; }
    public uint WebIconId { get; set; }
    public uint Notification { get; set; }
    public uint LocalizedLoreTextId { get; set; }
    public uint Points { get; set; }
    public uint Category { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public uint Id { get; set; }
    public byte IsCombatAward { get; set; }
    public byte IsAchievement { get; set; }
    public byte IsSecret { get; set; }
    public byte Frequency { get; set; }
}