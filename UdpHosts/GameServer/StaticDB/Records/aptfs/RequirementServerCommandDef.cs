namespace GameServer.Data.SDB.Records.aptfs;
public record class RequirementServerCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Local { get; set; }
    public byte LocalInit { get; set; }
    public byte Server { get; set; }
    public byte Client { get; set; }
}
