namespace GameServer.StaticDB.Records.customdata;

public record SlotAbilityCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint AbilityId { get; set; } = 0;
}