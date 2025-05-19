namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetByObjectTypeCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Projectile { get; set; }
    public byte Deployable { get; set; }
    public byte Tinyobject { get; set; }
    public byte FailNoTargets { get; set; }
    public byte Vehicle { get; set; }
    public byte Character { get; set; }
}