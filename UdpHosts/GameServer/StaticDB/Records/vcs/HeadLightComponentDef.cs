namespace GameServer.Data.SDB.Records.vcs;
public record class HeadLightComponentDef
{
    // public Vec4 Color { get; set; }
    public float FovX { get; set; }
    public float Softness { get; set; }
    public float AttenuationStart { get; set; }
    public string Hardpoint { get; set; }
    public float AttenuationEnd { get; set; }
    public uint Id { get; set; }
    public float FovY { get; set; }
}
