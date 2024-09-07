namespace GameServer.Data.SDB.Records.apt;
public record class RegisterRandomCommandDef : ICommandDef
{
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public uint Id { get; set; }
    public byte Regop { get; set; }
}