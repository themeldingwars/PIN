namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromItemStatCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public ushort Stat { get; set; }
    public byte FromTarget { get; set; }
    public byte FromInitiator { get; set; }
    public byte Regop { get; set; }
}