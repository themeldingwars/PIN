using Shared.Collision.Layers;

namespace Shared.Collision.Zone;

public class ZoneFile
{
    public byte[] NameBytes = [];
    private string _name = string.Empty;

    public string Magic { get; set; } = string.Empty;
    public int Version { get; set; }
    public long Timestamp { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            NameBytes = [];
        }
    }

    public WorldLayer? Root { get; set; }
}
