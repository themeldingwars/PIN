using GameServer.Entities;

namespace GameServer.Systems.Encounters;

public interface IInteractionHandler
{
    void OnInteraction(BaseEntity target);
}