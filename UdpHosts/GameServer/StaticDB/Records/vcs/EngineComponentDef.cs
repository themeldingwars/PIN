namespace GameServer.Data.SDB.Records.vcs;
public record class EngineComponentDef
{
    public float TorqueMaxRpm { get; set; }
    public float MaxTorque { get; set; }
    public float ResistanceOptRpm { get; set; }
    public float ResistanceMinRpm { get; set; }
    public float TorqueMinRpm { get; set; }
    public float MaxRpm { get; set; }
    public float ClutchSlipRpm { get; set; }
    public float ResistanceMaxRpm { get; set; }
    public float MinRpm { get; set; }
    public float OptRpm { get; set; }
    public uint Id { get; set; }
}