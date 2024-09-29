using System.Collections.Generic;
using Aero.Gen;
using AeroMessages.Common;

namespace GameServer.Systems.Encounters;

public interface IEncounter
{
    ulong EntityId { get; }
    EntityId AeroEntityId { get; }
    IShard Shard { get; }
    public IAeroEncounter View { get; }
    HashSet<INetworkPlayer> Participants { get; }

    void OnUpdate(ulong currentTime);
    void OnSignal();
    void OnSuccess();
    void OnFailure();

    // Vector3 Position { get; set; }
    // bool IsGlobalScope();
    // float GetScopeRange();
}