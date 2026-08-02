using System.Text;

namespace Shared.Collision.Layers;

public class MeldingPerimeterLayer : WorldLayer
{
    private bool _hasUnk2;
    public override uint TypeId => 5;
    public string Name { get; private set; } = string.Empty;
    public uint ControlPoints { get; private set; }
    public uint BitfieldBitLength { get; private set; }
    public byte[] Bitfield { get; private set; } = [];
    public uint Unk1 { get; private set; }
    public List<string> Perimeters { get; } = [];
    public byte Unk2 { get; private set; }
    public byte[] RemainingData { get; private set; } = [];

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        var dataEnd = reader.BaseStream.Position + length;
        DataLength = length;

        Name = reader.ReadLengthPrefixedString();
        ControlPoints = reader.ReadUInt32();
        BitfieldBitLength = reader.ReadUInt32();

        var bitfieldBytes = (int)((BitfieldBitLength + 7) / 8);
        Bitfield = reader.ReadBytes(bitfieldBytes);
        Unk1 = reader.ReadUInt32();

        var perimeterCount = reader.ReadUInt32();
        for (uint i = 0; i < perimeterCount; i++)
        {
            Perimeters.Add(reader.ReadLengthPrefixedString());
        }

        if (reader.BaseStream.Position < dataEnd)
        {
            Unk2 = reader.ReadByte();
            _hasUnk2 = true;
        }

        var remaining = (int)(dataEnd - reader.BaseStream.Position);
        if (remaining > 0)
        {
            RemainingData = reader.ReadBytes(remaining);
        }
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.WriteLengthPrefixedString(Name);
        writer.Write(ControlPoints);
        writer.Write(BitfieldBitLength);
        writer.Write(Bitfield);
        writer.Write(Unk1);
        writer.Write((uint)Perimeters.Count);
        foreach (var p in Perimeters)
        {
            writer.WriteLengthPrefixedString(p);
        }

        if (_hasUnk2)
        {
            writer.Write(Unk2);
        }

        if (RemainingData.Length > 0)
        {
            writer.Write(RemainingData);
        }
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        uint len = 4 + (uint)Encoding.UTF8.GetByteCount(Name);
        len += 4 + 4 + ((BitfieldBitLength + 7) / 8) + 4 + 4;
        foreach (var p in Perimeters)
        {
            len += 4 + (uint)Encoding.UTF8.GetByteCount(p);
        }

        if (_hasUnk2)
        {
            len += 1;
        }

        len += (uint)RemainingData.Length;
        return len;
    }
}

