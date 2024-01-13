namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromDamageCommandDef
{
    public uint Id { get; set; }
    public float Multiplier { get; set; }
    public byte Regop { get; set; }
}