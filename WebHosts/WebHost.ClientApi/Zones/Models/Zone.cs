namespace WebHost.ClientApi.Zones.Models;

public static class Zone
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