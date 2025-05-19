namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireCAISStateCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte None { get; set; }
    public byte Fatigued { get; set; }
    public byte Unhealthy { get; set; }
    public byte Healthy { get; set; }
}