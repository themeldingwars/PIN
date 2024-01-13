namespace GameServer.Data.SDB.Records.dbfabrication;
public record class ActionGroupDistSubGroup
{
    public uint ActionSubGroupId { get; set; }
    public float Probability { get; set; }
    public uint ActionGroupId { get; set; }
    public uint Id { get; set; }
    public byte IsAutogen { get; set; }
}
