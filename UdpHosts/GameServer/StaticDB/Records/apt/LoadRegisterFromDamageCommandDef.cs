namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromDamageCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public float Multiplier { get; set; }
    public byte Regop { get; set; }
}