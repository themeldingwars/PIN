namespace GameServer.Data.SDB.Records.customdata;

public record ModifyOwnerResourcesCommandDef : ICommandDef
{
    public uint Id { get; set; }
}