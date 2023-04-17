namespace WebHost.ClientApi.Armies.Models;

public class ArmyList
{
    public string Commander { get; set; }
    public string Link { get; set; }
    public string Name { get; set; }
    public string Personality { get; set; }
    public string Playstyle { get; set; }
    public string IsRecruitingStr { get; set; }
    public uint MemberCount { get; set; }
    public bool IsRecruiting { get; set; }
}