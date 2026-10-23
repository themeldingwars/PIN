namespace GameServer.StaticDB.Records.customdata;

/// <summary>
/// One entry of the server-side XP scaling curve: the multiplier applied to the base_xp of a
/// dbcharacter::XPRewardType for kills by NPCs (or deployables) whose level falls inside the range.
/// </summary>
public record XpLevelScale
{
    public uint MinLevel { get; set; }
    public uint MaxLevel { get; set; }
    public float Scale { get; set; }
}
