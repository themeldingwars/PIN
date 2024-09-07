namespace GameServer.Data.SDB.Records.aptfs;
public record class SetWeaponDamageTypeCommandDef : ICommandDef
{
    public float BonusAmt { get; set; }
    public uint Id { get; set; }
    public byte BonusAmtRegop { get; set; }
    public byte WeaponIndex { get; set; }
    public byte DamageType { get; set; }
}
