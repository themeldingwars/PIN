namespace GameServer.Data.SDB.Records.apt;
public record class PushRegisterCommandDef
{
    public uint Id { get; set; }
    public byte Regop { get; set; }
}