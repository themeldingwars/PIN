namespace GameServer.Data.SDB.Records.customdata;

public record class AuthorizeTerminalCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public int TerminalId { get; set; }
    public int TerminalType { get; set; }
}