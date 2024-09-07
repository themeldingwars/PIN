namespace GameServer.Data.SDB.Records.apt;
public record class PushRegisterCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Regop { get; set; }
}