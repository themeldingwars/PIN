namespace GameServer.Data.SDB.Records.aptfs;
public record class SetProjectileTargetCommandDef : ICommandDef
{
    public uint ForAmmoType { get; set; }
    public uint Id { get; set; }
    public byte SetNpcTarget { get; set; }
    public byte TargetingThis { get; set; }
}
