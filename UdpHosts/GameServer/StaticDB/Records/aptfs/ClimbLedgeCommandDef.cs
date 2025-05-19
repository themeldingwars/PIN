namespace GameServer.Data.SDB.Records.aptfs;
public record class ClimbLedgeCommandDef : ICommandDef
{
    public float Maxheight { get; set; }
    public uint Id { get; set; }
    public byte Testonly { get; set; }
    public byte Alwaysjump { get; set; }
    public byte Frompermission { get; set; }
    public byte Checkpermission { get; set; }
}