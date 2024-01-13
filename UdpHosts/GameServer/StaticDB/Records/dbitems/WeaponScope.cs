namespace GameServer.Data.SDB.Records.dbitems;
public record class WeaponScope
{
    public uint FirstpersonCziMapOverrideId { get; set; }
    public uint NightvisionMode { get; set; }
    public uint CziMapOverrideId { get; set; }
    public float Maxzoom { get; set; }
    public uint VisualrecId { get; set; }
    public float Zoomstep { get; set; }
    public float Zoomjittery { get; set; }
    public uint FirstpersonVisualrecId { get; set; }
    public uint Statusfx { get; set; }
    public float Zoomjitterx { get; set; }
    public float Minzoom { get; set; }
    public uint Id { get; set; }
}
