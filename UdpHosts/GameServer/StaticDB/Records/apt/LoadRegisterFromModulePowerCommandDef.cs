namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromModulePowerCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte ModulePowerType { get; set; }
    public byte Regop { get; set; }
}