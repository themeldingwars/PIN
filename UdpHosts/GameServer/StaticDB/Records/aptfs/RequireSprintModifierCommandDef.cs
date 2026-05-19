namespace GameServer.StaticDB.Records.aptfs;
public record class RequireSprintModifierCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Negate { get; set; }
}