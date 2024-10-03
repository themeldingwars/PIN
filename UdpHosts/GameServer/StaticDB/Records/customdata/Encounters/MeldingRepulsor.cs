using System.Numerics;

namespace GameServer.Data.SDB.Records.customdata;

public record MeldingRepulsor
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; }
    public string PerimiterSetName { get; set; }
    public Deployable Terminal { get; set; }
    public Deployable Repulsor { get; set; }
    public MeldingPosition MeldingPosition { get; set; }
}

public record MeldingPosition
{
    public uint ControlPointIndex { get; set; }
    public Vector3 Position;
}
