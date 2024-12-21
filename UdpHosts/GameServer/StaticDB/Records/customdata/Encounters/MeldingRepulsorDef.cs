using System.Numerics;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.StaticDB.Records.customdata.Encounters;

public record MeldingRepulsorDef : IEncounterDef
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
