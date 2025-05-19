namespace GameServer.Data.SDB.Records.dbitems;
public record class ResourceNodeBeacon
{
    public uint LocalizedNameId { get; set; }
    public uint AnimationNetwork { get; set; }
    public uint Visualrecord { get; set; }
    public uint Calldownfx { get; set; }
    public uint SendBackInteractString { get; set; }
    public uint PosefileId { get; set; }
    public float Scale { get; set; }
    public uint HackInteractString { get; set; }
    public uint CallingDownMarker { get; set; }
    public uint ExplosionEffect { get; set; }
    public uint Id { get; set; }
    public byte Tier { get; set; }
}