namespace GameServer.Data.SDB.Records.aptfs;
public record class SwitchWeaponCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte Forced { get; set; }
    public byte RestoreOnRollback { get; set; }
    public sbyte TargetWeaponSlot { get; set; }
    public byte PlayAnimation { get; set; }
}