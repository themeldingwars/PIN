namespace GameServer.Data.SDB.Records.aptfs;
public record class RegisterMovementEffectCommandDef : ICommandDef
{
    public uint StatusfxId { get; set; }
    public uint MovestateIndex { get; set; }
    public uint Id { get; set; }
    public byte OnClient { get; set; }
    public byte Sprinting { get; set; }
    public byte Reapply { get; set; }
    public byte OnServer { get; set; }
}