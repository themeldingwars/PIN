namespace GameServer.Data.SDB.Records.dbencounterdata;
public record class MapMarkerInfo
{
    // public Vec4 tint { get; set; }
    public uint IconId { get; set; }
    public uint Locname { get; set; }
    public uint Id { get; set; }
    public ushort Stage4RadioId { get; set; }
    public ushort Stage2RadioId { get; set; }
    public ushort Stage3RadioId { get; set; }
    public ushort Stage5RadioId { get; set; }
    public ushort IntroRadioId { get; set; }
    public byte ShowNavigation { get; set; }
    public byte HideCasing { get; set; }
    public sbyte BroadcastPriority { get; set; }
    public byte IgnoreSIN { get; set; }
    public byte ShowWaypoint { get; set; }
}