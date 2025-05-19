namespace GameServer.Data.SDB.Records.dbitems;
public record class SkinOverride
{
    public uint NameId { get; set; }
    public uint VisualGroupId { get; set; }
    public uint ItemId { get; set; }
    public uint Id { get; set; }
    public byte ItemType { get; set; }
}