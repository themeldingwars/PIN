namespace GameServer.Data.SDB.Records.aptfs;
public record class CopyInitiationPositionCommandDef
{

    public uint Id { get; set; }
    public uint SrcEffectId { get; set; }
    public byte FailNoEffect { get; set; }
}
