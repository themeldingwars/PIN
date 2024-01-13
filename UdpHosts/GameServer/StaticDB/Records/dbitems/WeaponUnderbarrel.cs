namespace GameServer.Data.SDB.Records.dbitems;
public record class WeaponUnderbarrel
{
    public uint FirstpersonCziMapOverrideId { get; set; }
    public uint CziMapOverrideId { get; set; }
    public uint VisualrecId { get; set; }
    public uint FirstpersonVisualrecId { get; set; }
    public uint WeaponTypeId { get; set; }
    public uint Id { get; set; }
}
