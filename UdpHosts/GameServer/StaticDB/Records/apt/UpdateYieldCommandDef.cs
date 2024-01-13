namespace GameServer.Data.SDB.Records.apt;
public record class UpdateYieldCommandDef
{
    public uint Id { get; set; }
    public uint Period { get; set; }
    public byte Regop { get; set; }
}