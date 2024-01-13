namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireSquadLeaderCommandDef
{
    public uint Id { get; set; }
    public byte FailNone { get; set; }
    public byte Negate { get; set; }
}
