namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromItemStatCommandDef
{
    public uint Id { get; set; }
    public ushort Stat { get; set; }
    public byte FromTarget { get; set; }
    public byte FromInitiator { get; set; }
    public byte Regop { get; set; }
}