namespace GameServer.StaticDB.Records.dbfabrication;
public record class ActionGroupDistAction
{
    public float Probability { get; set; }
    public uint ActionId { get; set; }
    public uint ActionGroupId { get; set; }
    public uint Id { get; set; }
}