namespace GameServer.StaticDB.Records.dbcharacter;
public record class FactionGroupMembers
{
    public uint FactiongroupId { get; set; }
    public uint FactionId { get; set; }
    public float ReputationMult { get; set; }
}