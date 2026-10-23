using System.IO;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.Character;
using Serilog;

namespace GameServer.Entities.Character;

public record WarpaintTatoo
{
    public uint Id;
    public uint Color;
    public Vector4[] Transform;
    public byte Usage;
}

public class WarpaintTatooData
{
    public WarpaintTatoo[] Entries;

    public static WarpaintTatooData Parse(byte[] data)
    {
        var ok = TryParse(data, out WarpaintTatooData result);
        if (ok)
        {
            return result;
        }
        else
        {
            return new WarpaintTatooData();
        }
    }

    public static bool TryParse(byte[] data, out WarpaintTatooData result)
    {
        result = new WarpaintTatooData();
        try
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var count = reader.ReadUInt32();
            result.Entries = new WarpaintTatoo[count];

            for (int i = 0; i < count; i++)
            {
                result.Entries[i] = new()
                {
                    Id = reader.ReadUInt32(),
                    Transform = [
                        new(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()),
                        new(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle()),
                        new(
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle(),
                            reader.ReadSingle())
                    ],
                    Usage = reader.ReadByte(),
                };
            }

            return true;
        }
        catch
        {
            Log.Debug($"Failed to parse WarpaintTatooData, len={data.Length}");
            return false;
        }
    }

    public VisualsDecalsBlock[] ToAero()
    {
        if (Entries != null && Entries.Length != 0)
        {
            var blocks = new VisualsDecalsBlock[Entries.Length];
            for (int i = 0; i < Entries.Length; i++)
            {
                blocks[i].DecalId = Entries[i].Id;
                blocks[i].Color = Entries[i].Color;
                blocks[i].Transform = new HalfVector4[3];
                blocks[i].Transform[0] = Entries[i].Transform[0];
                blocks[i].Transform[1] = Entries[i].Transform[1];
                blocks[i].Transform[2] = Entries[i].Transform[2];
                blocks[i].Usage = Entries[i].Usage;
            }

            return blocks;
        }
        else
        {
            return [];
        }
    }
}