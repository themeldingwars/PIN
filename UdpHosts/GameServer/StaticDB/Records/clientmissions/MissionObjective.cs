namespace GameServer.Data.SDB.Records.clientmissions;
public record class MissionObjective
{
    public uint WaypointId { get; set; }
    public uint MissionId { get; set; }
    public string Description { get; set; }
    public string Name { get; set; }
    public uint Order { get; set; }
    public byte HasUiPopup { get; set; }
    public byte ShowSINTrail { get; set; }
}