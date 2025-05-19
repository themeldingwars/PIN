namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetClassTypeCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Classtype { get; set; }
    public byte Negate { get; set; }
}