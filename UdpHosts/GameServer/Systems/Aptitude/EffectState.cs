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
}