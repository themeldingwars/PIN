namespace GameServer;

// See the wiki: https://github.com/themeldingwars/Documentation/wiki/Interaction-Types
public enum InteractionType : byte
{
    Execute = 1,
    Revive = 2,
    Vehicle = 3,
    Doctor = 4,
    Transport = 5,
    Repair = 6,
    Hack = 7,
    Generic = 8,
    List = 9,
    Collect = 10,
    GenericHold = 11,
    Vendor = 12,
    Search = 13,
    Grab = 14,
    Holstertalk = 15
}