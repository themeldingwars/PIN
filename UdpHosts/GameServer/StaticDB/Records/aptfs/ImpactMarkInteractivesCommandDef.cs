namespace GameServer.Data.SDB.Records.aptfs;
public record class ImpactMarkInteractivesCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Unmark { get; set; }
}