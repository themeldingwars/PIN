using System.Collections.Generic;
using AeroMessages.GSS.V66.Character.Command;
using GameServer.Entities;

namespace GameServer.Systems.Encounters;

public interface IInteractionHandler
{
    void OnInteraction(BaseEntity actingEntity, BaseEntity target);
}

public interface IDonationHandler
{
    void OnDonation(UiQueryResponse response, INetworkPlayer player);
}

public interface IProximityHandler
{
    public HashSet<INetworkPlayer> Participants { get; }
    void OnProximity(BaseEntity sourceEntity, INetworkPlayer player);
}

public interface IExitAttachmentHandler
{
    void OnExitAttachment(BaseEntity targetEntity, INetworkPlayer player);
}