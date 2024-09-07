namespace GameServer.Data.SDB.Records.aptfs;
public record class BombardmentCommandDef : ICommandDef
{
    public int Ratevariance { get; set; }
    public float Rise { get; set; }
    public uint Ammotype { get; set; }
    public float Range { get; set; }
    public uint Burstcount { get; set; }
    public float Elevation { get; set; }
    public uint Firerate { get; set; }
    public float Damage { get; set; }
    public float Spread { get; set; }
    public uint Id { get; set; }
}
