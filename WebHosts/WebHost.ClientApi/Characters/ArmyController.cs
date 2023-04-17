using Microsoft.AspNetCore.Mvc;
using System;

namespace WebHost.ClientApi.Characters;

public class ArmyController : ControllerBase
{
    [Route("api/v3/armies")]
    [HttpGet]
    public object GetArmies()
    {
        // Querystring q=&page=1&per_page=11
        PageResults pageResults = new PageResults()
        {
            Page = 1,
            TotalCount = 1,
            Results = new ArmyList[]
            {
                new ArmyList()
                {
                    Commander = "Fallback",
                    Link = "/armies/",
                    Name = "ARMY Corp",
                    Personality = "Moderate",
                    Playstyle = "pve",
                    IsRecruitingStr = "Yes",
                    MemberCount = 3,
                    IsRecruiting = true
                },
                new ArmyList()
                {
                    Commander = "TestUser10",
                    Link = "/armies/",
                    Name = "Test Army",
                    Personality = "Casual",
                    Playstyle = "pve",
                    IsRecruitingStr = "No",
                    MemberCount = 1,
                    IsRecruiting = false
                }
            }
        };

        return pageResults;
    }

    [Route("api/v3/armies/{armyId}")]
    [HttpGet]
    public object GetArmy(string armyId)
    {
        if (string.IsNullOrEmpty(armyId)) { return new { }; }

        Army army = new Army()
        {
            Id = null,
            AccountId = null,
            ArmyGuid = ulong.Parse(armyId),
            CharacterGuid = null,
            Name = "Army Corp",
            Description = "Description of [ARMY].",
            Playstyle = "pve",
            Personality = "moderate",
            Motd = "Pink Fluffy Unicorns!",
            IsRecruiting = true,
            CreatedAt = null,
            UpdatedAt = null,
            CommanderGuid = null,
            TagPosition = null,
            MinSize = null,
            MaxSize = null,
            Disbanded = false,
            Website = null,
            MassEmail = null,
            Region = null,
            LoginMessage = null,
            Timezone = null,
            EstablishedAt = 1364160283,
            Tag = "ARMY",
            Language = null,
            DefaultRankId = null,
            MemberCount = null,
            Officers = new Officers[]
            {
                new Officers()
                {
                    RankName = "Commander",
                    IsOnline = true,
                    Name = "Fallback"
                },
                new Officers()
                {
                    RankName = "Officer",
                    IsOnline = null,
                    Name = "TestUser1"
                },
                new Officers()
                {
                    RankName = "Officer",
                    IsOnline = false,
                    Name = "TestUser2"
                }
            }
        };
        return army;
    }

    [Route("api/v3/armies/{armyId}/members")]
    [HttpGet]
    [Produces("application/json")]
    public object GetArmyMembers(string armyId)
    {
        // Querystring page=1&per_page=100 
        if (string.IsNullOrEmpty(armyId)) { return new { }; }

        PageResults PageResults = new PageResults()
        {
            Page = 1,
            TotalCount = 3,
            Results = new ArmyMember[]
            {
                new ArmyMember()
                {
                    Id = null,
                    ArmyId = null,
                    ArmyGuid = ulong.Parse(armyId),
                    CharacterGuid = 9168405683928077054,
                    ArmyRankId = 165531,
                    CreatedAt = 0,
                    UpdatedAt = 0,
                    RankName = "Commander",
                    RankPosition = 1,
                    LastZoneId = 448,
                    LastSeenAt = 1430591716,
                    CurrentLevel = 20,
                    CurrentFrameSdbId = 76337,
                    IsOnline = true,
                    PublicNote = null,
                    OfficerNote = null,
                    Name = "Fallback"
                },
                new ArmyMember()
                {
                    Id = null,
                    ArmyId = null,
                    ArmyGuid = ulong.Parse(armyId),
                    CharacterGuid = 9162788533740412926,
                    ArmyRankId = 165631,
                    CreatedAt = 0,
                    UpdatedAt = 0,
                    RankName = "Officer",
                    RankPosition = 2,
                    LastZoneId = 448,
                    LastSeenAt = 1408555500,
                    CurrentLevel = 15,
                    CurrentFrameSdbId = null,
                    IsOnline = true,
                    PublicNote = null,
                    OfficerNote = null,
                    Name = "TestUser1"
                },
                new ArmyMember()
                {
                    Id = null,
                    ArmyId = null,
                    ArmyGuid = ulong.Parse(armyId),
                    CharacterGuid = 9153042507174448638,
                    ArmyRankId = 165731,
                    CreatedAt = 0,
                    UpdatedAt = 0,
                    RankName = "Soldier",
                    RankPosition = 2,
                    LastZoneId = 1030,
                    LastSeenAt = 1408555500,
                    CurrentLevel = 18,
                    CurrentFrameSdbId = null,
                    IsOnline = true,
                    PublicNote = null,
                    OfficerNote = null,
                    Name = "TestUser1"
                }
            }
        };

        return PageResults;
    }

    [Route("api/v3/armies/{armyId}/applications")]
    [HttpGet]
    public object GetArmyMemberApplications(string armyId)
    {
        if (string.IsNullOrEmpty(armyId)) { return new { }; }

        ArmyApplication[] armyApplications = new ArmyApplication[]
        {
            new ArmyApplication()
            {
                Id = 15063821,
                ArmyId = null,
                CharacterGuid = 2329448601048208126,
                Message = "",
                Direction = "apply",
                CreatedAt = null,
                UpdatedAt = null,
                ArmyGuid = ulong.Parse(armyId),
                CurrentLevel = null,
                CurrentFrameSdbId = null,
                IsOnline = true,
                Name = "TestUser10"
            },
            new ArmyApplication()
            {
                Id = 12944121,
                ArmyId = null,
                CharacterGuid = 2329448601048208126,
                Message = "Hello, i would like to join the main army.",
                Direction = "invite",
                CreatedAt = null,
                UpdatedAt = null,
                ArmyGuid = ulong.Parse(armyId),
                CurrentLevel = 20,
                CurrentFrameSdbId = 82360,
                IsOnline = true,
                Name = "TestUser11"
            }
        };

        return armyApplications;
    }

    [Route("api/v3/armies/{armyId}/members/ranks")]
    [HttpGet]
    public object GetArmyRanks(string armyId)
    {
        if (string.IsNullOrEmpty(armyId)) { return new { }; }

        ArmyRank[] armyRanks = new ArmyRank[]
        {
            new ArmyRank
            {
                Id = 165531,
                ArmyId = null,
                ArmyGuid = ulong.Parse(armyId),
                Name = "Commander",
                IsCommander = true,
                CanInvite = true,
                CanKick = true,
                CreatedAt = null,
                UpdatedAt = null,
                CanEdit = true,
                CanPromote = true,
                Position = 1,
                IsOfficer = true,
                CanEditMotd = true,
                CanMassEmail = false,
                IsDefault = false
            },
            new ArmyRank
            {
                Id = 165631,
                ArmyId = null,
                ArmyGuid = ulong.Parse(armyId),
                Name = "Officer",
                IsCommander = null,
                CanInvite = true,
                CanKick = true,
                CreatedAt = null,
                UpdatedAt = null,
                CanEdit = false,
                CanPromote = false,
                Position = 2,
                IsOfficer = true,
                CanEditMotd = true,
                CanMassEmail = false,
                IsDefault = false
            },
            new ArmyRank
            {
                Id = 165731,
                ArmyId = null,
                ArmyGuid = ulong.Parse(armyId),
                Name = "Soldier",
                IsCommander = null,
                CanInvite = false,
                CanKick = false,
                CreatedAt = null,
                UpdatedAt = null,
                CanEdit = false,
                CanPromote = false,
                Position = 3,
                IsOfficer = null,
                CanEditMotd = false,
                CanMassEmail = false,
                IsDefault = true
            }
        };

        return armyRanks;
    }

    [Route("api/v3/armies/{armyId}/members/{rankId}/rank")]
    [HttpGet]
    public object GetArmyRankAccess(string armyId, string rankId)
    {
        if (string.IsNullOrEmpty(armyId) || string.IsNullOrEmpty(rankId)) { return new { }; }

        ArmyRank armyRank = new ArmyRank()
        {
            Id = 165531,
            ArmyId = 59331,
            ArmyGuid = ulong.Parse(armyId),
            Name = "Commander",
            IsCommander = true,
            CanInvite = true,
            CanKick = true,
            CreatedAt = 1364002829,
            UpdatedAt = 1364002829,
            CanEdit = true,
            CanPromote = true,
            Position = 1,
            IsOfficer = true,
            CanEditMotd = true,
            CanMassEmail = false,
            IsDefault = null
        };

        return armyRank;
    }
}

public class PageResults
{
    public uint Page { get; set; }
    public uint TotalCount { get; set; }
    public Array Results { get; set; }
}
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
    public uint? MaxSize { get; set;}
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

public class Officers
{
    public string RankName { get; set; }
    public bool? IsOnline { get; set; }
    public string Name { get; set; }
}

public class ArmyMember
{
    public uint? Id { get; set; }
    public uint? ArmyId { get; set; }
    public ulong ArmyGuid { get; set; }
    public ulong CharacterGuid { get; set; }
    public uint ArmyRankId { get; set; }
    public uint CreatedAt { get; set; }
    public uint UpdatedAt { get; set; }
    public string RankName { get; set; }
    public uint RankPosition { get; set; }
    public uint LastZoneId { get; set; }
    public uint LastSeenAt { get; set; }
    public uint? CurrentLevel { get; set; }
    public uint? CurrentFrameSdbId { get; set; }
    public bool? IsOnline { get; set; }
    public string PublicNote { get; set; }
    public string OfficerNote { get; set; }
    public string Name { get; set; }
}

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