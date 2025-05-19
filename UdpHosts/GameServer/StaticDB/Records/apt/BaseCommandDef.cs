namespace GameServer.Data.SDB.Records.apt;
public record class BaseCommandDef : ICommandDef
{
    public uint Next { get; set; }
    public uint Subtype { get; set; }
    public uint Id { get; set; }
}