namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class GlobeViewLocation
{
    // public Vec2 Location { get; set; }
    public uint LevelBandId { get; set; }
    public string Thumbnail1 { get; set; }
    public uint NameLocId { get; set; }
    public uint StatusLocId { get; set; }
    public string WebImage1 { get; set; }
    public uint PvpLocId { get; set; }
    public uint Population { get; set; }
    public uint SovereigntyLocId { get; set; }
    public uint ClimateLocId { get; set; }
    public uint ZoneId { get; set; }
    public uint RouteMask { get; set; }
    public uint DescriptionLocId { get; set; }
    public string WebImage3 { get; set; }
    public string Thumbnail2 { get; set; }
    public string WebImage2 { get; set; }
    public uint TextureId { get; set; }
    public string Thumbnail3 { get; set; }
    public byte Type { get; set; }
}