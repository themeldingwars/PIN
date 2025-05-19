namespace GameServer.Data.SDB.Records.dbcharacter;
public record class VoiceSet
{
    public uint DisplayFlags { get; set; }
    public uint Group { get; set; }
    public uint Id { get; set; }
    public uint LocalizedNameId { get; set; }
    public byte Sex { get; set; }
}