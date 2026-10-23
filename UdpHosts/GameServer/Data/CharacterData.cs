using GrpcCharacter = GrpcGameServerAPIClient.CharacterAndBattleframeVisuals;
using GrpcInventoryResponse = GrpcGameServerAPIClient.CharacterInventoryResponse;
using GrpcProgressionResponse = GrpcGameServerAPIClient.BattleframeProgressionResponse;
using GrpcUnlocksResponse = GrpcGameServerAPIClient.CharacterUnlocksResponse;

namespace GameServer.Data;

public sealed record CharacterData(
    GrpcCharacter Character,
    GrpcInventoryResponse Inventory,
    GrpcUnlocksResponse Unlocks,
    GrpcProgressionResponse Battleframes)
{
    public bool FetchFailed { get; init; }

    /// <summary>
    ///     Whether RIN returned enough to build the character without reaching for the hardcoded fallback data
    /// </summary>
    public bool IsUsable => !FetchFailed && Character != null && CharacterInventory.IsUsable(Inventory);
}
