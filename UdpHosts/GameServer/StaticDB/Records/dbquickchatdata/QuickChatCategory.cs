namespace GameServer.Data.SDB.Records.dbquickchatdata;
public record class QuickChatCategory
{
    public uint NameId { get; set; }
    public uint Keybinding { get; set; }
    public uint Id { get; set; }
    public byte Channel { get; set; }
}