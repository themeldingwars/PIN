namespace GameServer.Data.SDB.Records.vcs;
public record class DeployableComponentDef
{
    public uint CloakEffectId { get; set; }
    public uint DeployableType { get; set; }
    public uint UncloakEffectId { get; set; }
    public string AnchorHardpoint { get; set; }
    public string Hardpoint { get; set; }
    public uint Id { get; set; }
}
