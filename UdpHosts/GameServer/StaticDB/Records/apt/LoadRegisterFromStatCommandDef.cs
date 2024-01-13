namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromStatCommandDef
{
    public uint Id { get; set; }
    public ushort Stat { get; set; }
    public byte Regop { get; set; }
}