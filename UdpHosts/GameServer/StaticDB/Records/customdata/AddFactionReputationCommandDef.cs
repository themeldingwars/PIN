namespace GameServer.StaticDB.Records.customdata;

public record AddFactionReputationCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint FactionId { get; set; }
    public uint Amount { get; set; }
}