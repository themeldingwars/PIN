namespace GameServer.StaticDB.Records.clientmissions;
public record class MissionGiver
{
    public uint MissionId { get; set; }
    public uint MonsterId { get; set; }
}