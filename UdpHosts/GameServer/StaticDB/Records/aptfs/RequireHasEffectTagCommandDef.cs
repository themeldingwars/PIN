namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireHasEffectTagCommandDef
{
    public uint StackCount { get; set; }
    public uint TagId { get; set; }
    public uint Id { get; set; }
    public byte Negate { get; set; }
    public byte AllowPrediction { get; set; }
}
