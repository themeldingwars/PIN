using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcGameServerAPIClient;

namespace GameServer.GRPC;

public static class GRPCService
{
    private static GrpcChannel _channel;
    private static GameServerAPI.GameServerAPIClient _client;

    public static void Init(string address)
    {
        _channel = GrpcChannel.ForAddress(address);
        _client = new GameServerAPI.GameServerAPIClient(_channel);
    }

    public static bool IsAvailable()
    {
        return _channel.State == Grpc.Core.ConnectivityState.Ready;
    }

    public static async Task<CharacterAndBattleframeVisuals> GetCharacterAndBattleframeVisualsAsync(long characterId)
    {
        return await _client.GetCharacterAndBattleframeVisualsAsync(new CharacterID { ID = characterId });
    }
}