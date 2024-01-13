namespace GameServer.Data.SDB.Records.clientmissions;
public record class MissionWaypoint
{
    // public Vec3 location { get; set; }
    // public Vec2[] area_polygon { get; set; }
    public int ChunkId { get; set; }
    public int LocalizedNameId { get; set; }
    public int MapMarkerType { get; set; }
    public int Id { get; set; }
}
