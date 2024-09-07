namespace GameServer.Data.SDB.Records.aptfs;
public record class CombatFlagsCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte ReversedControls { get; set; }
    public byte ImmuneFalldamage { get; set; }
    public byte ImmunePhysics { get; set; }
    public byte RestrictMovement { get; set; }
    public byte RestrictWeapon { get; set; }
    public byte RestrictAbilities { get; set; }
    public byte KnockDown { get; set; }
    public byte MoveThroughObjects { get; set; }
    public byte RestrictSprint { get; set; }
    public byte RestrictMelee { get; set; }
    public byte RestrictInteraction { get; set; }
    public byte ImmuneDeath { get; set; }
    public byte RemoveHitboxes { get; set; }
    public byte RestrictStumble { get; set; }
}
