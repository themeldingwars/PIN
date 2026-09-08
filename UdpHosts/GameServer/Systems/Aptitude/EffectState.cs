namespace GameServer.Systems.Aptitude;

public class EffectState
{
    public Effect Effect;
    public Context Context;
    public byte Index;
    public uint Time;
    public ulong LastUpdateTime;
    public byte Stacks = 1;
    public bool MaxStacksExceeded;

    // A tick iterates a snapshot. A removal chain can remove another effect in that snapshot and reuse its
    // slot; never execute or remove the old state a second time.
    public bool Removed;
}