namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireArmyCommandDef
{
    public uint Id { get; set; }
    public byte CheckTarget { get; set; }
    public byte Rank { get; set; }
}
