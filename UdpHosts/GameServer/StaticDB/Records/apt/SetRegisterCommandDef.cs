namespace GameServer.Data.SDB.Records.apt;
public record class SetRegisterCommandDef
{
    public uint Id { get; set; }
    public float RegisterVal { get; set; }
    public byte Regop { get; set; }
}