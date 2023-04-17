using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using WebHost.ClientApi.Models.Zone;

namespace WebHost.ClientApi.Controllers;

[ApiController]
public class MiscController : ControllerBase
{
    private static ConcurrentDictionary<uint, ZoneSettings> _zones;

    [Route("api/v1/zones/queue_ids")]
    [HttpGet]
    public object ZoneQueue()
    {
        return new { };
    }

    [Route("api/v2/zone_settings")]
    [HttpGet]
    [Produces("application/json")]
    public object ZoneSettings()
    {
        _zones = new ConcurrentDictionary<uint, ZoneSettings>();

        var zoneTemp1 = new ZoneSettings
                        {
                            Id = 2821,
                            ZoneId = 1051,
                            Description = "Baneclaw",
                            QueueingEnabled = true,
                            ChallengeEnabled = false,
                            SkipMatchmaking = true,
                            TeamCount = 1,
                            MinPlayersPerTeam = 10,
                            MinPlayersAcceptPerTeam = 0,
                            MaxPlayersPerTeam = 20,
                            ChallengeMinPlayersPerTeam = 0,
                            ChallengeMaxPlayersPerTeam = 0,
                            Gametype = Zone.GameType.RAID.ToString(),
                            DisplayedName = "MATCH_MAP_BOSS_BANECLAW",
                            DisplayedGametype = "MATCH_MAP_BOSS_BANECLAW",
                            RotationPriority = 1,
                            MissionId = null,
                            SortOrder = null,
                            XpBonus = 0.0f,
                            InstanceTypePool = Zone.InstancePoolType.PVE.ToString(),
                            CertRequired = false,
                            IsPreviewZone = false,
                            DisplayedDesc = "MATCH_MAP_BOSS_BANECLAW_DESC",
                            RewardWinner =
                                new RewardWinner
                                {
                                    Items = Array.Empty<Array>(), Loots = Array.Empty<Array>()
                                },
                            RewardLoser =
                                new RewardLoser
                                {
                                    Items = Array.Empty<Array>(), Loots = Array.Empty<Array>()
                                },
                            Images =
                                new ZoneImages
                                {
                                    Thumbnail = "/assets/zones/baneclaw/tbn.png",
                                    Screenshot =
                                        new[]
                                        {
                                            "/assets/zones/baneclaw/01.png",
                                            "/assets/zones/baneclaw/02.png",
                                            "/assets/zones/baneclaw/03.png"
                                        },
                                    Lfg = "/assets/zones/placeholder-ss.png"
                                },
                            CertRequirements =
                                new object[]
                                {
                                    new CertRequirements
                                    {
                                        Id = 1021,
                                        ZoneSettingId = 2821,
                                        Presence = "present",
                                        CertId = 784,
                                        AuthorizePosition = "all",
                                        DifficultyKey = null
                                    }
                                },
                            DifficultyLevels = new object[]
                                               {
                                                   new DifficultyLevels
                                                   {
                                                       Id = 1021,
                                                       ZoneSettingId = 2821,
                                                       UiString =
                                                           Zone.UIString.INSTANCE_DIFFICULTY_NORMAL
                                                               .ToString(),
                                                       MinLevel = 37,
                                                       DifficultyKey =
                                                           Zone.DifficultyKey.NORMAL_MODE.ToString(),
                                                       MinPlayers = 20,
                                                       MinPlayersAccept = 20,
                                                       MaxPlayers = 20,
                                                       GroupMinPlayers = 10,
                                                       GroupMaxPlayers = 20,
                                                       DisplayLevel = 37,
                                                       MaxSuggestedLevel = 40
                                                   }
                                               }
                        };
        _zones.AddOrUpdate(zoneTemp1.Id, zoneTemp1, (k, nc) => nc);

        var zoneTemp2 = new ZoneSettings
                        {
                            Id = 2721,
                            ZoneId = 1022,
                            Description = "Kanaloa",
                            QueueingEnabled = true,
                            ChallengeEnabled = false,
                            SkipMatchmaking = true,
                            TeamCount = 1,
                            MinPlayersPerTeam = 10,
                            MinPlayersAcceptPerTeam = 0,
                            MaxPlayersPerTeam = 20,
                            ChallengeMinPlayersPerTeam = 0,
                            ChallengeMaxPlayersPerTeam = 0,
                            Gametype = Zone.GameType.RAID.ToString(),
                            DisplayedName = "MATCH_MAP_BOSS_KANALOA",
                            DisplayedGametype = "MATCH_MAP_BOSS_KANALOA",
                            RotationPriority = 1,
                            MissionId = null,
                            SortOrder = null,
                            XpBonus = 0.0f,
                            InstanceTypePool = Zone.InstancePoolType.PVE.ToString(),
                            CertRequired = false,
                            IsPreviewZone = false,
                            DisplayedDesc = "MATCH_MAP_BOSS_KANALOA_DESC",
                            RewardWinner =
                                new RewardWinner
                                {
                                    Items = Array.Empty<Array>(), Loots = Array.Empty<Array>()
                                },
                            RewardLoser =
                                new RewardLoser
                                {
                                    Items = Array.Empty<Array>(), Loots = Array.Empty<Array>()
                                },
                            Images =
                                new ZoneImages
                                {
                                    Thumbnail = "/assets/zones/kanaloa/tbn.png",
                                    Screenshot =
                                        new[]
                                        {
                                            "/assets/zones/kanaloa/01.png", "/assets/zones/kanaloa/02.png",
                                            "/assets/zones/kanaloa/03.png"
                                        },
                                    Lfg = "/assets/zones/placeholder-ss.png"
                                },
                            CertRequirements =
                                new object[]
                                {
                                    new CertRequirements
                                    {
                                        Id = 1321,
                                        ZoneSettingId = 2721,
                                        Presence = "present",
                                        CertId = 785,
                                        AuthorizePosition = "all",
                                        DifficultyKey = Zone.DifficultyKey.NORMAL_MODE.ToString()
                                    },
                                    new CertRequirements
                                    {
                                        Id = 1421,
                                        ZoneSettingId = 2721,
                                        Presence = "present",
                                        CertId = 4905,
                                        AuthorizePosition = "all",
                                        DifficultyKey = Zone.DifficultyKey.HARD_MODE.ToString()
                                    }
                                },
                            DifficultyLevels = new object[]
                                               {
                                                   new DifficultyLevels
                                                   {
                                                       Id = 3721,
                                                       ZoneSettingId = 2721,
                                                       UiString =
                                                           Zone.UIString.INSTANCE_DIFFICULTY_NORMAL
                                                               .ToString(),
                                                       MinLevel = 39,
                                                       DifficultyKey =
                                                           Zone.DifficultyKey.NORMAL_MODE.ToString(),
                                                       MinPlayers = 20,
                                                       MinPlayersAccept = 20,
                                                       MaxPlayers = 20,
                                                       GroupMinPlayers = 10,
                                                       GroupMaxPlayers = 20,
                                                       DisplayLevel = 39,
                                                       MaxSuggestedLevel = 40
                                                   },
                                                   new DifficultyLevels
                                                   {
                                                       Id = 3821,
                                                       ZoneSettingId = 2721,
                                                       UiString =
                                                           Zone.UIString.INSTANCE_DIFFICULTY_HARD
                                                               .ToString(),
                                                       MinLevel = 39,
                                                       DifficultyKey =
                                                           Zone.DifficultyKey.HARD_MODE.ToString(),
                                                       MinPlayers = 20,
                                                       MinPlayersAccept = 20,
                                                       MaxPlayers = 20,
                                                       GroupMinPlayers = 10,
                                                       GroupMaxPlayers = 20,
                                                       DisplayLevel = 39,
                                                       MaxSuggestedLevel = 40
                                                   }
                                               }
                        };
        _zones.AddOrUpdate(zoneTemp2.Id, zoneTemp2, (k, nc) => nc);

        return _zones.Values;
    }
}