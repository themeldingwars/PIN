namespace GameServer.Data.SDB.Records.apt;
public record class SetRegisterCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public float RegisterVal { get; set; }
    public byte Regop { get; set; }
}