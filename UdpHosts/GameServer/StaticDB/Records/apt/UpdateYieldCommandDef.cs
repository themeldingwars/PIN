namespace GameServer.Data.SDB.Records.apt;
public record class UpdateYieldCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint Period { get; set; }
    public byte Regop { get; set; }
}