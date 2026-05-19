namespace GameServer.StaticDB.Records.aptfs;
public record class RequireFriendsCommandDef : ICommandDef
{
    public uint Friends { get; set; }
    public uint Id { get; set; }
}