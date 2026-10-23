using Aero.Protocol;

namespace GameServer.Handlers.Gss.Character;

/// <summary>
///     Handlers for the character namespace typecode itself. No message currently rides it, so the class
///     exists to make the route known and to give unhandled messages on it a name in the logs.
/// </summary>
[Typecode(GssTables.Ns.Character)]
public static class Character_Root
{
}
