namespace GameServer.Data.SDB.Records.dbcharacter;
public record class EmoteRecord
{
    // public Vec3 Collisionoffset { get; set; }
    public uint Flags { get; set; }
    public string AnimationName { get; set; }
    public uint HeadAnimOverrideId { get; set; }
    public uint Statuseffect { get; set; }
    public string Name { get; set; }
    public uint AnimOverrideId { get; set; }
    public ushort Id { get; set; }
    
}
