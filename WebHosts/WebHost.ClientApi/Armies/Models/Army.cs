using System;

namespace WebHost.ClientApi.Armies.Models;

public class Army
{
    public uint? Id { get; set; }
    public uint? AccountId { get; set; }
    public ulong ArmyGuid { get; set; }
    public ulong? CharacterGuid { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Playstyle { get; set; }
    public string Personality { get; set; }
    public string Motd { get; set; }
    public bool IsRecruiting { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public ulong? CommanderGuid { get; set; }
    public string TagPosition { get; set; }
    public uint? MinSize { get; set; }
    public uint? MaxSize { get; set; }
    public bool? Disbanded { get; set; }
    public string Website { get; set; }
    public bool? MassEmail { get; set; }
    public string Region { get; set; }
    public string LoginMessage { get; set; }
    public string Timezone { get; set; }
    public uint EstablishedAt { get; set; }
    public string Tag { get; set; }
    public string Language { get; set; }
    public ulong? DefaultRankId { get; set; }
    public uint? MemberCount { get; set; }
    public Array Officers { get; set; }
}