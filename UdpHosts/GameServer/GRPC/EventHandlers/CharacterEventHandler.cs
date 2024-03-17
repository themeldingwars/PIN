using System.Collections.Generic;
using System.Linq;
using GrpcGameServerAPIClient;

namespace GameServer.GRPC.EventHandlers;

public static class CharacterEventHandler
{
    public static void HandleEvent(CharacterVisualsUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        clients.Values.FirstOrDefault(p => p.CharacterId + 0xFE == e.CharacterGuid)
               ?.CharacterEntity.LoadRemote(e.CharacterAndBattleframeVisuals);
    }
}