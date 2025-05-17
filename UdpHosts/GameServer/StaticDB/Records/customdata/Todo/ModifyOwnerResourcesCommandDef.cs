namespace GameServer.Data.SDB.Records.customdata;

public record ModifyOwnerResourcesCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint ResourceSdbId { get; set; } = 0;
    public int Quantity { get; set; } = 0;
}