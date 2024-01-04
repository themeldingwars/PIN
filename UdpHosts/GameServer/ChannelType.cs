#nullable enable
namespace GameServer;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "See the wiki for more details on how the channels differ: https://github.com/themeldingwars/Documentation/wiki/Game-Server-Protocol-Overview#main-connection")]
public enum ChannelType : byte
{
    Control = 0,
    Matrix = 1,
    ReliableGss = 2,
    UnreliableGss = 3
}