namespace GameServer.StaticDB.Records.dbcharacter;
public record class VoiceGroup
{
    public uint Id { get; set; }
    public byte PlayerSelectable { get; set; }
}