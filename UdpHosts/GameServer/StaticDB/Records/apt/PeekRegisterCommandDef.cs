namespace GameServer.Data.SDB.Records.apt;
public record class PeekRegisterCommandDef
{
    public uint Id { get; set; }
    public byte Regop { get; set; }
}