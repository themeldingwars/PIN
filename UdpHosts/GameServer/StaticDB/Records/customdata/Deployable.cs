using System.Numerics;

namespace GameServer.Data.SDB.Records.customdata;
public record class Deployable
{
    public uint Id { get; set; }
    public uint ZoneId { get; set; }

    public uint Type { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Orientation { get; set; }
}