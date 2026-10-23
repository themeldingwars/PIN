using Aero.Protocol;
using AeroMessages.GSS.Character.Command;
using GameServer.Extensions;
using GameServer.Protocol;

namespace GameServer.Handlers.Gss.Character;

/// <summary>
///     Handlers for the spectator controller view, the view a character is on while spectating.
/// </summary>
[Typecode(GssCharacterView.SpectatorController)]
public static class Character_SpectatorController
{
    [MessageID(GssCharacterCommand.PerformTextChat)]
    public static void PerformTextChat(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<PerformTextChat>();
        var character = player.CharacterEntity;
        shard.Chat.CharacterPerformTextChat(client, character, query);
    }
}
