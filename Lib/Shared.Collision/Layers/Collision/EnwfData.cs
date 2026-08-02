namespace Shared.Collision.Layers;

public struct IndiceTri
{
    public ushort A;
    public ushort B;
    public ushort C;
}

public class EnwfData
{
    public byte[] Magic = [];
    public ushort Version;
    public ushort Revision;
    public uint Id;
    public uint[] PhysicsMatIds = [];

    public EnwfVertBlock[] VertBlocks = [];
    public EnwfIndiceBlock[] IndiceBlocks = [];
    public EnwfMatItem[] MatItems = [];
    public EnwfMoppBlock[] MoppBlocks = [];

    public byte[] HavokBinaryTagfile = [];
}

public class EnwfVertBlock
{
    public Vec3[] Vertices = [];
}

public class EnwfIndiceBlock
{
    public uint IndiceType;
    public IndiceTri[] Indices = [];
}

public class EnwfMatItem
{
    public uint Id;
    public byte[] Data = [];
}

public class EnwfMoppBlock
{
    public Vec4 Floats;
    public uint DataSize;
    public byte[] Data = [];
    public byte Unk1;
    public ushort Unk2;
    public ushort[] Shorts = [];
}
