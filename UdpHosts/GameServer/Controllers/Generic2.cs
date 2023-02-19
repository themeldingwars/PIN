using GameServer.Enums.GSS.Generic;
using GameServer.Packets;

namespace GameServer.Controllers;

[ControllerID(Enums.GSS.Controllers.Generic2)]
public class Generic2 : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard)
    {
    }

    [MessageID((byte)Commands.UIToEncounterMessage)]
    public void UiToEncounter(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.ServerProfiler_RequestNames)]
    public void ServerProfiler_RequestNames(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.LocalProximityAbilitySuccess)]
    public void LocalProximityAbilitySuccess(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.RemoteProximityAbilitySuccess)]
    public void RemoteProximityAbilitySuccess(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.TrailRequest)]
    public void TrailRequest(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.RequestLeaveZone)]
    public void RequestLeaveZone(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.RequestLogout)]
    public void RequestLogout(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.RequestEncounterInfo)]
    public void RequestEncounterInfo(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.RequestActiveEncounters)]
    public void RequestActiveEncounters(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }


    [MessageID((byte)Commands.VotekickRequest)]
    public void VotekickRequest(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.VotekickResponse)]
    public void VotekickResponse(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.GlobalCounterRequest)]
    public void GlobalCounterRequest(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.CurrentLoadoutRequest)]
    public void CurrentLoadoutRequest(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.VendorProductRequest)]
    public void VendorProductRequest(INetworkClient client, IPlayer player, ulong EntityID, GamePacket packet)
    {
    }
}