namespace GameServer.Data.SDB.Records.dbzonemetadata;
public record class ChunkRecord
{
    public ulong ExcludeFromPathing { get; set; } // Ehh?
    public uint CoordY { get; set; }
    public uint CoordX { get; set; }
    public uint Id { get; set; }
    public byte Cubeface { get; set; }
    public byte RemoveInProduction { get; set; }
}
