namespace GameServer.Data.SDB.Records.dbcharacter;
public record class GibVisuals
{
    public uint? ChangeHeadVisuals { get; set; }
    public uint? ChangeHeadAcc1Visuals { get; set; }
    public uint DeathAnimIndex { get; set; }
    public uint TransitionPfxId { get; set; }
    public float BlastImpulseStrength { get; set; }
    public uint? ChangeBattleframeVisuals { get; set; }
    public uint? DirectVrecId { get; set; }
    public uint? ChangeBackpackVisuals { get; set; }
    public uint? ChangeHeadAcc2Visuals { get; set; }
    public uint Id { get; set; }
    public byte AllowOrnaments { get; set; }
}
