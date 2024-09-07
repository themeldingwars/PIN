namespace GameServer.Data.SDB.Records.aptfs;
public record class RemoveClientStatusEffectCommandDef : ICommandDef
{
    public uint StatusEffectId { get; set; }
    public uint Id { get; set; }
    public byte ApplyToSelf { get; set; }
    public byte ForcePrediction { get; set; }
    public byte UseTargetClients { get; set; }
}
