using Aero.Protocol;
using GameServer.Protocol;

namespace GameServer.Handlers.Gss.Character;

/// <summary>
///     Handlers for the character mission and marker controller view.
/// </summary>
[Typecode(GssCharacterView.MissionAndMarkerController)]
public static class Character_MissionAndMarkerController
{
    [MessageID(GssCharacterCommand.RequestAllAchievements)]
    public static void RequestAllAchievements(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }

    [MessageID(GssCharacterCommand.TryResumeTutorialChain)]
    public static void TryResumeTutorialChain(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: Implement
    }
}
