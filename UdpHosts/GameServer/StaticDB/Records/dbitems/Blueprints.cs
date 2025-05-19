namespace GameServer.Data.SDB.Records.dbitems;
public record class Blueprints
{
    public uint BuildTimeSecs { get; set; }
    public uint MainOutputItemId { get; set; }
    public uint HeadBlueprintId { get; set; }
    public uint RequiredFactionId { get; set; }
    public uint ResearchBlueprintId { get; set; }
    public uint Id { get; set; }
    public byte BlueprintType { get; set; }
    public sbyte RequiredFactionStance { get; set; }
    public byte MaxParallel { get; set; }
}