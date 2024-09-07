namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromStatCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort Stat { get; set; }
    public byte Regop { get; set; }
}