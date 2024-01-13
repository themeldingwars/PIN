namespace GameServer.Data.SDB.Records.dbcharacter;
public record class CharCreateLoadout
{
    public uint FrameId { get; set; }
    public uint BaseFrameId { get; set; }
    public uint BaseCertificate { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public byte IsExperimental { get; set; }
    public byte IsStartingLoadout { get; set; }
    public byte IsDev { get; set; }
    public byte Archtype { get; set; }
}
