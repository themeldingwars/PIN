namespace GameServer.Data.SDB.Records.dbdialogdata;
public record class BattleChatterDescriptions
{
    public float SpreadMeters { get; set; }
    public uint DefaultDialogScriptId { get; set; }
    public uint DialogScriptSetId { get; set; }
    public uint DurationMs { get; set; }
    public uint Id { get; set; }
    public byte ProbabilityAlly { get; set; }
    public byte ProbabilityTargetedPlayer { get; set; }
    public byte ProbabilityHostile { get; set; }
    public byte Memoryless { get; set; }
}
