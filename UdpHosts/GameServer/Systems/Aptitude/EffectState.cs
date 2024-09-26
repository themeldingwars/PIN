namespace GameServer.Aptitude;

public class EffectState
{
    public Effect Effect;
    public Context Context;
    public byte Index = 0;
    public uint Time;
    public ulong LastUpdateTime = 0;
    public byte Stacks = 1;
    public bool MaxStacksExceeded = false;
}