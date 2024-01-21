using AeroMessages.GSS.V66;

namespace GameServer.Aptitude;

public class Effect
{
    public Data.SDB.Records.apt.StatusEffectData Data;
    public Chain ApplyChain;
    public Chain DurationChain;
    public Chain UpdateChain;
    public Chain RemoveChain;

    public uint Id => Data.Id;
    public uint UpdateFrequency => Data.UpdateFrequency;
    public uint MaxStackCount => Data.MaxStackCount;
}