namespace WebHost.ClientApi.Armies.Models;

public class ArmyApplication
{
    public uint Id { get; set; }
    public uint? ArmyId { get; set; }
    public ulong CharacterGuid { get; set; }
    public string Message { get; set; }
    public string Direction { get; set; }
    public uint? CreatedAt { get; set; }
    public uint? UpdatedAt { get; set; }
    public ulong ArmyGuid { get; set; }
    public uint? CurrentLevel { get; set; }
    public uint? CurrentFrameSdbId { get; set; }
    public bool? IsOnline { get; set; }
    public string Name { get; set; }
}