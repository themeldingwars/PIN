namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class TextureGradient
{
    // public Vec2 grad_end_point { get; set; }
    // public Vec2 grad_start_point { get; set; }
    // public Vec2[] vertices { get; set; }
    public uint SetId { get; set; }
    public uint GradStartColor { get; set; }
    public uint GradEndColor { get; set; }
    public byte[] Indices { get; set; }
}
