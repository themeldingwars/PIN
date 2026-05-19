namespace GameServer.StaticDB.Records.apt;
public record class UpdateWaitCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public uint Duration { get; set; }
    public byte Regop { get; set; }
}