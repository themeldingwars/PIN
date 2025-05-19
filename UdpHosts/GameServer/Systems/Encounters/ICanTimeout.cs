namespace GameServer.Systems.Encounters;

public interface ICanTimeout
{
    ulong EntityId { get; }

    void OnTimeOut();
}