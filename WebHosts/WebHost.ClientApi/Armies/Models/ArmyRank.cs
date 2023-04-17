namespace WebHost.ClientApi.Armies.Models;

public class ArmyRank
{
    public uint Id { get; set; }
    public uint? ArmyId { get; set; }
    public ulong ArmyGuid { get; set; }
    public string Name { get; set; }
    public bool? IsCommander { get; set; }
    public bool CanInvite { get; set; }
    public bool CanKick { get; set; }
    public uint? CreatedAt { get; set; }
    public uint? UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanPromote { get; set; }
    public uint Position { get; set; }
    public bool? IsOfficer { get; set; }
    public bool CanEditMotd { get; set; }
    public bool CanMassEmail { get; set; }
    public bool? IsDefault { get; set; }
}