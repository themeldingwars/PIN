namespace GameServer.Data.SDB.Records.dbquickchatdata;
public record class QuickChatCommand
{
    public uint NameId { get; set; }
    public uint ParticleEffectId { get; set; }
    public uint SoundrecordId { get; set; }
    public uint Keybinding { get; set; }
    public uint EmoteId { get; set; }
    public uint UiId { get; set; }
    public uint CategoryId { get; set; }
    public uint Radius { get; set; }
    public uint Order { get; set; }
    public uint Id { get; set; }
}
