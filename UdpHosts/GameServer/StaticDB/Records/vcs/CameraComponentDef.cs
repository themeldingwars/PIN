namespace GameServer.Data.SDB.Records.vcs;
public record class CameraComponentDef
{
    public float MinFollowDist { get; set; }
    public float LpfRate { get; set; }
    public float ScrollAmount { get; set; }
    public float VerticalOffset { get; set; }
    public float MaxFov { get; set; }
    public float GoalDist { get; set; }
    public float LpfDamping { get; set; }
    public uint Role { get; set; }
    public float MinClipDist { get; set; }
    public float TurnResist { get; set; }
    public float MinFov { get; set; }
    public float MaxFollowDist { get; set; }
    public float FovTopSpeed { get; set; }
    public float ZoomDamping { get; set; }
    public float ZoomRate { get; set; }
    public uint Id { get; set; }
    public byte AllowFirstPerson { get; set; }
}
