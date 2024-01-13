namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireProjectileSlopeCommandDef
{
    public float Minslope { get; set; }
    public float Maxslope { get; set; }
    public uint Id { get; set; }
    public byte Negate { get; set; }
}
