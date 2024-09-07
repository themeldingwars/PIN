namespace GameServer.Data.SDB.Records.aptfs;
public record class UiNamedVariableCommandDef : ICommandDef
{
    public string MemberName { get; set; }
    public string UiName { get; set; }
    public uint Id { get; set; }
    public ushort NameId { get; set; }
    public byte VarSrctype { get; set; }
}
