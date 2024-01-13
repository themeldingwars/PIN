namespace GameServer.Data.SDB.Records.aptfs;
public record class SlotAmmoCommandDef
{
    public uint AmmoType { get; set; }
    public float WeaponDamageAdd { get; set; }
    public uint ReplaceAmmoType { get; set; }
    public float WeaponDamageMult { get; set; }
    public uint Id { get; set; }
    public byte AltWeapon { get; set; }
    public byte RestoreOnRollback { get; set; }
    public byte TargetWeaponSlot { get; set; }
    public byte CreditToAbility { get; set; }
}
