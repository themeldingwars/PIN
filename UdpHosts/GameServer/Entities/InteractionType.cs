using System.Diagnostics.CodeAnalysis;

namespace GameServer;

[SuppressMessage("StyleCop.CSharp.DocumentationRules",
"SA1602:EnumerationItemsMustBeDocumented",
Justification = "We don't know much about these other than that they affect animations. See the wiki: https://github.com/themeldingwars/Documentation/wiki/Interaction-Types")]
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