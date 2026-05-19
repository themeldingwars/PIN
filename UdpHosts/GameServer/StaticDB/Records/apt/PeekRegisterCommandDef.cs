namespace GameServer.StaticDB.Records.apt;
public record class PeekRegisterCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Regop { get; set; }
}