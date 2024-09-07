namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetFriendliesCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte IncludeInitiator { get; set; }
    public byte IncludeOwner { get; set; }
    public byte FailNoTargets { get; set; }
    public byte IncludeSelf { get; set; }
}
