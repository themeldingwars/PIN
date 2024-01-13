namespace GameServer.Data.SDB.Records.aptfs;
public record class ApplyClientStatusEffectCommandDef
{
    public uint StatusEffectId { get; set; }
    public uint Id { get; set; }
    public byte ApplyToSelf { get; set; }
    public byte UseTargetClients { get; set; }
}
