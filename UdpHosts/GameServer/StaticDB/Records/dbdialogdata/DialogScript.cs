namespace GameServer.Data.SDB.Records.dbdialogdata;
public record class DialogScript
{
    public uint CharacterType { get; set; }
    public uint NextId { get; set; }
    public uint TextId { get; set; }
    public int DelayMs { get; set; }
    public ulong SoundEventId { get; set; }
    public uint EmoteId { get; set; }
    public uint Id { get; set; }
    public uint VoiceSet { get; set; }
    public byte Mood { get; set; }
    public byte PlayIncomingSignal { get; set; }
    public byte DisplayMode { get; set; }
    public byte Trigger { get; set; }
    public byte LookAtTarget { get; set; }
    public byte IsPublic { get; set; }
    public byte Priority { get; set; }
}
