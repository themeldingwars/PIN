namespace GameServer.Data.SDB.Records.dbdialogdata;
public record class BattleChatterSetParams
{
    public ulong SpeakerKey { get; set; }
    public ulong SetTags { get; set; }
    public ulong VoiceSetKey { get; set; }
    public uint SetId { get; set; }
    public uint DialogId { get; set; }
}
