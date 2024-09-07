namespace GameServer.Data.SDB.Records.aptfs;
public record class AddPhysicsCommandDef : ICommandDef
{
    public uint DamageResponse { get; set; }
    public uint Hitpoints { get; set; }
    public uint Id { get; set; }
    public ushort Posetype { get; set; }
    public byte HitpointsRegop { get; set; }
    public byte Aimorient { get; set; }
    public byte Staticobj { get; set; }
    public byte BlockEnemiesOnly { get; set; }
    public byte InheritScale { get; set; }
}
