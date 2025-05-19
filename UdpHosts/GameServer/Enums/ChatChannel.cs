namespace GameServer.Enums;

public enum ChatChannel : byte
{
    Invalid = 0,
    Zone = 1,
    Community = 2,
    Team = 3,
    ZoneLang = 4, // zone-en
    Encounter = 5,
    Say = 6, // local (also gets /emote messages)
    Yell = 7,
    Whisper = 8,
    Army = 9,
    Officer = 10,
    Debug = 11,
    Squad = 12,
    Platoon = 13,
    Friends = 14,
    SinSpeaker = 15,
    SinMessenger = 16,
    Admin = 17,
    Unknown = 18
}