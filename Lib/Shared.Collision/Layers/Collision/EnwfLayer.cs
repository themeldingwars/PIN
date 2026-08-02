namespace Shared.Collision.Layers;

public abstract class EnwfLayer : WorldLayer
{
    public EnwfData Enwf = new();

    public static EnwfData ParseRaw(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        var magic = reader.ReadBytes(4);
        var version = reader.ReadUInt16();
        var revision = reader.ReadUInt16();
        var id = reader.ReadUInt32();

        var numPhysicsMatIds = reader.ReadUInt32();
        var physicsMatIds = new uint[numPhysicsMatIds];
        for (int i = 0; i < numPhysicsMatIds; i++)
        {
            physicsMatIds[i] = reader.ReadUInt32();
        }

        var enwfData = new EnwfData
        {
            Magic = magic,
            Version = version,
            Revision = revision,
            Id = id,
            PhysicsMatIds = physicsMatIds,
            VertBlocks = [],
            IndiceBlocks = [],
            MatItems = [],
            MoppBlocks = [],
            HavokBinaryTagfile = []
        };

        if (revision == 2)
        {
            var numVertBlocks = reader.ReadUInt32();
            var vertBlocks = new EnwfVertBlock[numVertBlocks];
            for (int i = 0; i < numVertBlocks; i++)
            {
                var count = reader.ReadUInt32();
                var verts = new Vec3[count];
                for (int j = 0; j < count; j++)
                {
                    verts[j] = reader.ReadVec3();
                }

                vertBlocks[i] = new EnwfVertBlock { Vertices = verts };
            }

            enwfData.VertBlocks = vertBlocks;

            var numIndiceBlocks = reader.ReadUInt32();
            var indiceBlocks = new EnwfIndiceBlock[numIndiceBlocks];
            for (int i = 0; i < numIndiceBlocks; i++)
            {
                var count = reader.ReadUInt32();
                var indiceType = reader.ReadUInt32();
                var indices = new IndiceTri[count];
                for (int j = 0; j < count; j++)
                {
                    if (indiceType == 0x060002)
                    {
                        indices[j] = new IndiceTri
                        {
                            A = reader.ReadUInt16(),
                            B = reader.ReadUInt16(),
                            C = reader.ReadUInt16()
                        };
                    }
                    else if (indiceType == 0x030001)
                    {
                        indices[j] = new IndiceTri
                        {
                            A = reader.ReadByte(),
                            B = reader.ReadByte(),
                            C = reader.ReadByte()
                        };
                    }
                }

                indiceBlocks[i] = new EnwfIndiceBlock
                {
                    Indices = indices,
                    IndiceType = indiceType
                };
            }

            enwfData.IndiceBlocks = indiceBlocks;

            var numMatItems = reader.ReadUInt32();
            var matItems = new EnwfMatItem[numMatItems];
            for (int i = 0; i < numMatItems; i++)
            {
                var dataLen = reader.ReadUInt32();
                var matId = reader.ReadUInt32();
                var itemData = reader.ReadBytes((int)dataLen);
                matItems[i] = new EnwfMatItem { Id = matId, Data = itemData };
            }

            enwfData.MatItems = matItems;

            var numMoppBlocks = reader.ReadUInt32();
            var moppBlocks = new EnwfMoppBlock[numMoppBlocks];
            for (int i = 0; i < numMoppBlocks; i++)
            {
                var floats = reader.ReadVec4();
                var dataSize = reader.ReadUInt32();
                var blockData = reader.ReadBytes((int)dataSize);
                var unk1 = reader.ReadByte();
                var unk2 = reader.ReadUInt16();
                var numShorts = reader.ReadUInt32();
                var shorts = new ushort[numShorts];
                for (int j = 0; j < numShorts; j++)
                {
                    shorts[j] = reader.ReadUInt16();
                }

                moppBlocks[i] = new EnwfMoppBlock
                {
                    Floats = floats,
                    DataSize = dataSize,
                    Data = blockData,
                    Unk1 = unk1,
                    Unk2 = unk2,
                    Shorts = shorts
                };
            }

            enwfData.MoppBlocks = moppBlocks;
        }

        var remaining = (int)(stream.Length - stream.Position);
        if (remaining > 0)
        {
            enwfData.HavokBinaryTagfile = reader.ReadBytes(remaining);
        }

        return enwfData;
    }

    public override void ParseData(BinaryReader reader, uint length, WorldParseContext ctx)
    {
        var dataEnd = reader.BaseStream.Position + length;
        DataLength = length;

        Enwf.Magic = reader.ReadBytes(4);
        Enwf.Version = reader.ReadUInt16();
        Enwf.Revision = reader.ReadUInt16();
        Enwf.Id = reader.ReadUInt32();

        var numPhysicsMatIds = reader.ReadUInt32();
        Enwf.PhysicsMatIds = new uint[numPhysicsMatIds];
        for (int i = 0; i < numPhysicsMatIds; i++)
        {
            Enwf.PhysicsMatIds[i] = reader.ReadUInt32();
        }

        if (Enwf.Revision == 2)
        {
            var numVertBlocks = reader.ReadUInt32();
            Enwf.VertBlocks = new EnwfVertBlock[numVertBlocks];
            for (int i = 0; i < numVertBlocks; i++)
            {
                var count = reader.ReadUInt32();
                var block = new EnwfVertBlock
                {
                    Vertices = new Vec3[count]
                };
                for (int j = 0; j < count; j++)
                {
                    block.Vertices[j] = reader.ReadVec3();
                }

                Enwf.VertBlocks[i] = block;
            }

            var numIndiceBlocks = reader.ReadUInt32();
            Enwf.IndiceBlocks = new EnwfIndiceBlock[numIndiceBlocks];
            for (int i = 0; i < numIndiceBlocks; i++)
            {
                var block = new EnwfIndiceBlock();
                var count = reader.ReadUInt32();
                block.IndiceType = reader.ReadUInt32();
                block.Indices = new IndiceTri[count];
                for (int j = 0; j < count; j++)
                {
                    if (block.IndiceType == 0x060002)
                    {
                        block.Indices[j] = new IndiceTri
                        {
                            A = reader.ReadUInt16(),
                            B = reader.ReadUInt16(),
                            C = reader.ReadUInt16()
                        };
                    }
                    else if (block.IndiceType == 0x030001)
                    {
                        block.Indices[j] = new IndiceTri
                        {
                            A = reader.ReadByte(),
                            B = reader.ReadByte(),
                            C = reader.ReadByte()
                        };
                    }
                    else
                    {
                        throw new InvalidDataException($"Unexpected indiceType 0x{block.IndiceType:X}");
                    }
                }

                Enwf.IndiceBlocks[i] = block;
            }

            var numMatItems = reader.ReadUInt32();
            Enwf.MatItems = new EnwfMatItem[numMatItems];
            for (int i = 0; i < numMatItems; i++)
            {
                var item = new EnwfMatItem();
                var dataLen = reader.ReadUInt32();
                item.Id = reader.ReadUInt32();
                item.Data = reader.ReadBytes((int)dataLen);
                Enwf.MatItems[i] = item;
            }

            var numMoppBlocks = reader.ReadUInt32();
            Enwf.MoppBlocks = new EnwfMoppBlock[numMoppBlocks];
            for (int i = 0; i < numMoppBlocks; i++)
            {
                var block = new EnwfMoppBlock
                {
                    Floats = reader.ReadVec4(),
                    DataSize = reader.ReadUInt32()
                };
                block.Data = reader.ReadBytes((int)block.DataSize);
                block.Unk1 = reader.ReadByte();
                block.Unk2 = reader.ReadUInt16();
                var numShorts = reader.ReadUInt32();
                block.Shorts = new ushort[numShorts];
                for (int j = 0; j < numShorts; j++)
                {
                    block.Shorts[j] = reader.ReadUInt16();
                }

                Enwf.MoppBlocks[i] = block;
            }
        }

        var remaining = (int)(dataEnd - reader.BaseStream.Position);
        if (remaining > 0)
        {
            Enwf.HavokBinaryTagfile = reader.ReadBytes(remaining);
        }
    }

    public override void WriteData(BinaryWriter writer)
    {
        writer.Write(Enwf.Magic);
        writer.Write(Enwf.Version);
        writer.Write(Enwf.Revision);
        writer.Write(Enwf.Id);

        writer.Write((uint)Enwf.PhysicsMatIds.Length);
        foreach (var id in Enwf.PhysicsMatIds)
        {
            writer.Write(id);
        }

        if (Enwf.Revision == 2)
        {
            writer.Write((uint)Enwf.VertBlocks.Length);
            foreach (var block in Enwf.VertBlocks)
            {
                writer.Write((uint)block.Vertices.Length);
                foreach (var v in block.Vertices)
                {
                    writer.Write(v);
                }
            }

            writer.Write((uint)Enwf.IndiceBlocks.Length);
            foreach (var block in Enwf.IndiceBlocks)
            {
                writer.Write((uint)block.Indices.Length);
                writer.Write(block.IndiceType);
                foreach (var tri in block.Indices)
                {
                    if (block.IndiceType == 0x060002)
                    {
                        writer.Write(tri.A);
                        writer.Write(tri.B);
                        writer.Write(tri.C);
                    }
                    else if (block.IndiceType == 0x030001)
                    {
                        writer.Write((byte)tri.A);
                        writer.Write((byte)tri.B);
                        writer.Write((byte)tri.C);
                    }
                }
            }

            writer.Write((uint)Enwf.MatItems.Length);
            foreach (var item in Enwf.MatItems)
            {
                writer.Write((uint)item.Data.Length);
                writer.Write(item.Id);
                writer.Write(item.Data);
            }

            writer.Write((uint)Enwf.MoppBlocks.Length);
            foreach (var block in Enwf.MoppBlocks)
            {
                writer.Write(block.Floats);
                writer.Write(block.DataSize);
                writer.Write(block.Data);
                writer.Write(block.Unk1);
                writer.Write(block.Unk2);
                writer.Write((uint)block.Shorts.Length);
                foreach (var s in block.Shorts)
                {
                    writer.Write(s);
                }
            }
        }

        if (Enwf.HavokBinaryTagfile.Length > 0)
        {
            writer.Write(Enwf.HavokBinaryTagfile);
        }
    }

    public override uint ComputeDataLength()
    {
        if (DataLength > 0)
        {
            return DataLength;
        }

        uint len = 16;
        len += 4 + (uint)(Enwf.PhysicsMatIds.Length * 4);

        if (Enwf.Revision == 2)
        {
            len += 4;
            foreach (var block in Enwf.VertBlocks)
            {
                len += 4 + (uint)(block.Vertices.Length * 12);
            }

            len += 4;
            foreach (var block in Enwf.IndiceBlocks)
            {
                var bytesPerTri = block.IndiceType == 0x060002 ? 6 : 3;
                len += 8 + (uint)(block.Indices.Length * bytesPerTri);
            }

            len += 4;
            foreach (var item in Enwf.MatItems)
            {
                len += 8 + (uint)item.Data.Length;
            }

            len += 4;
            foreach (var block in Enwf.MoppBlocks)
            {
                len += 16 + 4 + (uint)block.Data.Length + 1 + 2 + 4 + (uint)(block.Shorts.Length * 2);
            }
        }

        len += (uint)Enwf.HavokBinaryTagfile.Length;
        return len;
    }
}
