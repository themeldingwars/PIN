namespace GameServer.Data.SDB.Records.vcs;
public record class CloakComponentDef
{
    public uint RaiseShieldTimems { get; set; }
    public uint LowerShieldTimems { get; set; }
    public uint CloakEffectId { get; set; }
    public uint UncloakEffectId { get; set; }
    public uint PoseFile { get; set; }
    public string Hardpoint { get; set; }
    public uint Id { get; set; }
}
