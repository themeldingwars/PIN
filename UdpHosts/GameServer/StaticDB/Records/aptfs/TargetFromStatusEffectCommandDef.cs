namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetFromStatusEffectCommandDef
{
    public uint StatusfxId { get; set; }
    public uint Id { get; set; }
    public byte AlsoInitiator { get; set; }
}
