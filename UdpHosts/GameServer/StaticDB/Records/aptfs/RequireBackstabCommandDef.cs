namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireBackstabCommandDef : ICommandDef
{
    public float Backangle { get; set; }
    public uint Id { get; set; }
}