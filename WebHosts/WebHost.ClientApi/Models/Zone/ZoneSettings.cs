using System;

namespace WebHost.ClientApi.Models.Zone;

static class Zone
{
    public enum GameType
    {
        INTRO,
        CAMPAIGN,
        MISSION,
        MELDINGFRAGMENT,
        RAID
    }

    public enum InstancePoolType
    {
        PVE,
        PVP
    }

    public enum DifficultyKey
    { 
        NORMAL_MODE,
        HARD_MODE
    }

    public enum UIString
    {
        INSTANCE_DIFFICULTY_NORMAL,
        INSTANCE_DIFFICULTY_HARD
    }
}

class ZoneSettings
{
    public uint Id { get; set; }
    public string Description { get; set; }
    public bool QueueingEnabled { get; set; }
    public bool ChallengeEnabled { get; set; }
    public bool SkipMatchmaking { get; set; }
    public uint TeamCount { get; set; }
    public uint MinPlayersPerTeam { get; set; }
    public uint MinPlayersAcceptPerTeam { get; set; }
    public uint MaxPlayersPerTeam { get; set; }
    public uint ChallengeMinPlayersPerTeam { get; set; }
    public uint ChallengeMaxPlayersPerTeam { get; set; }
    public string Gametype { get; set; }
    public string DisplayedName { get; set; }
    public string DisplayedGametype { get; set; }
    public uint RotationPriority { get; set; }
    public uint ZoneId { get; set; }
    public string MissionId { get; set; }
    public string SortOrder { get; set; }
    public float XpBonus { get; set; }
    public string InstanceTypePool { get; set; }
    public RewardWinner RewardWinner { get; set; }
    public RewardLoser RewardLoser { get; set; }
    public bool CertRequired { get; set; }
    public bool IsPreviewZone { get; set; }
    public string DisplayedDesc { get; set; }
    public ZoneImages Images { get; set; }
    public Array CertRequirements { get; set; }
    public Array DifficultyLevels { get; set; }

    /*
     * public string name { get; set; }
     * public ulong timestamp { get; set; }
     * public Dictionary<string, Vector3> pois { get; protected set; }
    */
}

public class RewardWinner
{
    public Array Items { get; set; }
    public Array Loots { get; set; }
}

public class RewardLoser
{
    public Array Items { get; set; }
    public Array Loots { get; set; }
}

public class ZoneImages
{
    public string Thumbnail { get; set; }
    public Array Screenshot { get; set; }
    public string Lfg { get; set; }
}

public class CertRequirements
{
    public uint Id { get; set; }
    public uint ZoneSettingId { get; set; }
    public string Presence { get; set; } // present
    public uint CertId { get; set; }
    public string AuthorizePosition { get; set; } // all
    public string DifficultyKey { get; set; }
}

public class DifficultyLevels
{
    public uint Id { get; set; }
    public uint ZoneSettingId { get; set; }
    public string UiString { get; set; }
    public uint MinLevel { get; set; }
    public string DifficultyKey { get; set; }
    public uint MinPlayers { get; set; }
    public uint MinPlayersAccept { get; set; }
    public uint MaxPlayers { get; set; }
    public uint GroupMinPlayers { get; set; }
    public uint GroupMaxPlayers { get; set; }
    public uint DisplayLevel { get; set; }
    public uint MaxSuggestedLevel { get; set; }
}