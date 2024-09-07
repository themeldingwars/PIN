namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromNamedVarCommandDef : ICommandDef
{
    public float UndeclValue { get; set; }
    public string MemberName { get; set; }
    public uint Id { get; set; }
    public ushort NameId { get; set; }
    public byte VarSrctype { get; set; }
    public byte Regop { get; set; }
}