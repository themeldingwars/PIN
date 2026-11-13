using System.Numerics;

namespace GameServer.StaticDB.Records.customdata.Encounters;

public interface IEncounterDef
{
}

public record SpawnPose
{
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; }
}
