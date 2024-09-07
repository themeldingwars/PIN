namespace GameServer.Data.SDB.Records.customdata;

public record RepositionClonesCommandDef : ICommandDef
{
    public uint Id { get; set; }
}