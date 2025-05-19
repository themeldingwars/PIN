namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class ZoneRecord
{
    public uint LocalizedNameId { get; set; }
    public uint Cubeface { get; set; }
    public string MainTitle { get; set; }
    public string SubTitle { get; set; }
    public uint LocalizedSubTitleId { get; set; }
    public uint LocalizedMinorTitleId { get; set; }
    public string MinorTitle { get; set; }
    public string Name { get; set; }
    public uint LocalizedMainTitleId { get; set; }
    public uint Id { get; set; }
    public ushort LevelBand { get; set; }
    public byte ZoneType { get; set; }
    public byte PreventSubZoneSpawns { get; set; }
}