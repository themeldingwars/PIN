namespace GameServer.Data.SDB.Records.apt;
public record class PopRegisterCommandDef
{
    public uint Id { get; set; }
    public byte Regop { get; set; }
}