namespace GameServer.StaticDB.Records.customdata;

public record ApplyPermanentEffectCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint EffectId { get; set; }
    public uint DurationSeconds { get; set; }
}