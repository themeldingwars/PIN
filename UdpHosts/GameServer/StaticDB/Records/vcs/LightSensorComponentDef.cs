namespace GameServer.Data.SDB.Records.vcs;
public record class LightSensorComponentDef
{
    public float RaycastLength { get; set; }
    public float SunAngle { get; set; }
    public uint Id { get; set; }
}