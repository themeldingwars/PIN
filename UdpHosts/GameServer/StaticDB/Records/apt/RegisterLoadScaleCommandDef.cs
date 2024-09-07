namespace GameServer.Data.SDB.Records.apt;
public record class RegisterLoadScaleCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Regop { get; set; }
}