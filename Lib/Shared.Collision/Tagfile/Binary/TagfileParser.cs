using System.Collections;

namespace Shared.Collision.Tagfile.Binary;

public static class TagfileParser
{
    public enum ControlTag
    {
        TAG_NONE = 0,
        TAG_FILE_INFO = 1,
        TAG_METADATA = 2,
        TAG_OBJECT = 3,
        TAG_OBJECT_REMEMBER = 4,
        TAG_OBJECT_BACKREF = 5,
        TAG_OBJECT_NULL = 6,
        TAG_FILE_END = 7,
    }

    public enum DataType
    {
        TYPE_VOID = 0,
        TYPE_BYTE,
        TYPE_INT,
        TYPE_REAL,
        TYPE_VEC_4,
        TYPE_VEC_8,
        TYPE_VEC_12,
        TYPE_VEC_16,
        TYPE_OBJECT,
        TYPE_STRUCT,
        TYPE_CSTRING,
        TYPE_NUM_BASIC_TYPES,
        TYPE_MASK_BASIC_TYPES = 0xf,

        TYPE_ARRAY = 0x10,
        TYPE_ARRAY_BYTE = TYPE_ARRAY | TYPE_BYTE,
        TYPE_ARRAY_INT = TYPE_ARRAY | TYPE_INT,
        TYPE_ARRAY_REAL = TYPE_ARRAY | TYPE_REAL,
        TYPE_ARRAY_VEC_4 = TYPE_ARRAY | TYPE_VEC_4,
        TYPE_ARRAY_VEC_8 = TYPE_ARRAY | TYPE_VEC_8,
        TYPE_ARRAY_VEC_12 = TYPE_ARRAY | TYPE_VEC_12,
        TYPE_ARRAY_VEC_16 = TYPE_ARRAY | TYPE_VEC_16,
        TYPE_ARRAY_OBJECT = TYPE_ARRAY | TYPE_OBJECT,
        TYPE_ARRAY_STRUCT = TYPE_ARRAY | TYPE_STRUCT,
        TYPE_ARRAY_CSTRING = TYPE_ARRAY | TYPE_CSTRING,

        TYPE_TUPLE = 0x20,
        TYPE_TUPLE_BYTE = TYPE_TUPLE | TYPE_BYTE,
        TYPE_TUPLE_INT = TYPE_TUPLE | TYPE_INT,
        TYPE_TUPLE_REAL = TYPE_TUPLE | TYPE_REAL,
        TYPE_TUPLE_VEC_4 = TYPE_TUPLE | TYPE_VEC_4,
        TYPE_TUPLE_VEC_8 = TYPE_TUPLE | TYPE_VEC_8,
        TYPE_TUPLE_VEC_12 = TYPE_TUPLE | TYPE_VEC_12,
        TYPE_TUPLE_VEC_16 = TYPE_TUPLE | TYPE_VEC_16,
        TYPE_TUPLE_OBJECT = TYPE_TUPLE | TYPE_OBJECT,
        TYPE_TUPLE_STRUCT = TYPE_TUPLE | TYPE_STRUCT,
        TYPE_TUPLE_CSTRING = TYPE_TUPLE | TYPE_CSTRING,
    }

    public static bool DataTypeIsArray(DataType subtype) => (subtype & DataType.TYPE_ARRAY) == DataType.TYPE_ARRAY;

    public static bool DataTypeIsTuple(DataType subtype) => (subtype & DataType.TYPE_TUPLE) == DataType.TYPE_TUPLE;

    public static bool DataTypeIsStruct(DataType subtype) => (subtype & DataType.TYPE_MASK_BASIC_TYPES) == DataType.TYPE_STRUCT;

    public static bool DataTypeIsObject(DataType subtype) => (subtype & DataType.TYPE_MASK_BASIC_TYPES) == DataType.TYPE_OBJECT;

    public static bool DataTypeHasClassname(DataType subtype) => DataTypeIsStruct(subtype) || DataTypeIsObject(subtype);

    public static bool DataTypeHasSize(DataType subtype) => DataTypeIsTuple(subtype);

    public static bool DataTypeHasArrayPrefix(DataType subtype)
    {
        var masked = subtype & DataType.TYPE_MASK_BASIC_TYPES;
        return masked == DataType.TYPE_INT || masked == DataType.TYPE_VEC_4;
    }

    public static string ObjectRefName(uint objectRef)
    {
        return $"X{objectRef:D4}";
    }

    public static BinaryTagfile ParseTagfile(BinaryReader reader)
    {
        var data = new BinaryTagfile();
        int objectCounter = 1;

        reader.BaseStream.Seek(8, SeekOrigin.Current);

        bool hitEnd = false;
        do
        {
            byte v = reader.ReadByte();
            ControlTag tag = (ControlTag)(v >> 1);
            switch (tag)
            {
                case ControlTag.TAG_FILE_INFO:
                    data.Version = (byte)(reader.ReadByte() >> 1);
                    break;

                case ControlTag.TAG_METADATA:
                    ParseMetadata(reader, data);
                    break;

                case ControlTag.TAG_OBJECT_REMEMBER:
                    var parsed = ParseObject(reader, data, ref objectCounter);
                    data.ObjectRoot ??= parsed; // First parsed object becomes root
                    break;

                case ControlTag.TAG_FILE_END:
                    hitEnd = true;
                    break;

                default:
                    throw new NotImplementedException($"Encountered unhandled tag {tag} at offset {reader.BaseStream.Position}");
            }
        }
        while (reader.BaseStream.Position != reader.BaseStream.Length && !hitEnd);

        return data;
    }

    private static (uint, bool) ParseVarInt(BinaryReader reader)
    {
        byte input;
        bool hasMore;
        uint result = 0;
        byte index = 0;
        do
        {
            input = reader.ReadByte();
            hasMore = (byte)(input >> 7) == 1;
            result = ((input & 0x7Fu) << (7 * index)) | result;
            index++;
        }
        while (hasMore);

        return (result >> 1, (byte)(result & 0x01) == 1);
    }

    private static string ParseString(BinaryReader reader, BinaryTagfile data)
    {
        var (result, isRef) = ParseVarInt(reader);

        if (result < 2 && isRef)
        {
            throw new DataMisalignedException($"ParseString read VarInt with Value {result} and Flag {isRef}");
        }

        if (isRef)
        {
            return data.Strings.GetValueOrDefault(result) ?? throw new DataMisalignedException($"Failed to get referenced string");
        }
        else
        {
            string str = new(reader.ReadChars((int)result));
            uint key = (uint)(2 + data.Strings.Count);
            data.Strings.Add(key, str);
            return str;
        }
    }

    private static ClassData ParseMetadata(BinaryReader reader, BinaryTagfile data)
    {
        ClassData meta = new ClassData
        {
            Name = ParseString(reader, data),
            Version = (byte)(reader.ReadByte() >> 1)
        };

        (uint metaRef, bool unknownFlagParent) = ParseVarInt(reader);
        meta.Parent = data.Metadata.GetValueOrDefault(metaRef);
        if (unknownFlagParent)
        {
            throw new NotImplementedException($"Encountered flag on parent parsing Metadata related to {meta.Name} at offset {reader.BaseStream.Position}. Value is {metaRef}");
        }

        (uint declaredMembers, bool unknownFlagMembers) = ParseVarInt(reader);
        if (unknownFlagMembers)
        {
            throw new NotImplementedException($"Encountered flag on declaredMembers when parsing Metadata related to {meta.Name} at offset {reader.BaseStream.Position}. Value is {declaredMembers}");
        }

        meta.Members = new Member[declaredMembers];
        for (int i = 0; i < declaredMembers; i++)
        {
            meta.Members[i] = ParseMetadataMember(reader, data);
        }

        uint key = (uint)(1 + data.Metadata.Count);
        data.Metadata.Add(key, meta);
        return meta;
    }

    private static Member ParseMetadataMember(BinaryReader reader, BinaryTagfile data)
    {
        Member member = new Member
        {
            Name = ParseString(reader, data),
            SubType = (DataType)(reader.ReadByte() >> 1),
        };

        if (DataTypeHasSize(member.SubType))
        {
            member.CArraySize = (byte)(reader.ReadByte() >> 1);
        }

        if (DataTypeHasClassname(member.SubType))
        {
            member.ClassName = ParseString(reader, data);
        }

        return member;
    }

    private static ObjectData ParseObject(BinaryReader reader, BinaryTagfile data, ref int objectCounter)
    {
        var (metaRef, unknownFlagMeta) = ParseVarInt(reader);
        if (unknownFlagMeta)
        {
            throw new NotImplementedException($"Encountered flag on metaRef at offset {reader.BaseStream.Position}. Value is {metaRef}");
        }

        ClassData meta = data.Metadata.GetValueOrDefault(metaRef) ?? throw new InvalidProgramException($"Tried to find meta from ref {metaRef} but got null");

        var obj = (ObjectData)ParseObject(reader, meta, data);
        obj.BinIdx = (uint)(1 + data.Objects.Count);
        data.Objects.Add((uint)(1 + data.Objects.Count), obj);
        objectCounter++;
        return obj;
    }

    private static ObjectData ParseObjectInline(BinaryReader reader, BinaryTagfile data)
    {
        var (metaRef, _) = ParseVarInt(reader);
        ClassData meta = data.Metadata.GetValueOrDefault(metaRef) ?? throw new InvalidProgramException($"Tried to find meta from ref {metaRef} but got null");

        return (ObjectData)ParseObject(reader, meta, data);
    }

    private static object ParseObject(BinaryReader reader, ClassData meta, BinaryTagfile data, bool asStructArray = false, int structArrayLength = 0)
    {
        Queue dataMembers = GetAllDataMembers(meta);

        int dataBitfieldByteCount = (int)Math.Ceiling((double)(dataMembers.Count / 8f));
        byte[] dataBitfield = reader.ReadBytes(dataBitfieldByteCount);

        byte bitValue = 1;
        byte byteIndex = 0;

        if (asStructArray)
        {
            object[][] memberArrays = new object[dataMembers.Count][];
            bool[] memberPresent = new bool[dataMembers.Count];
            string[] memberNames = new string[dataMembers.Count];
            int mIdx = 0;

            do
            {
                var member = (Member)dataMembers.Dequeue()!;
                bool present = (dataBitfield[byteIndex] & bitValue) != 0;
                memberNames[mIdx] = member.Name;
                memberPresent[mIdx] = present;

                if (present)
                {
                    byte parsedUnkPrefixByte = 8;
                    if (DataTypeHasArrayPrefix(member.SubType))
                    {
                        parsedUnkPrefixByte = reader.ReadByte();
                    }

                    object[] dataArr = new object[structArrayLength];
                    for (int i = 0; i < structArrayLength; i++)
                    {
                        if (DataTypeIsStruct(member.SubType) || DataTypeIsObject(member.SubType) || DataTypeIsTuple(member.SubType))
                        {
                            dataArr[i] = ParseObjectData(reader, member, data, parsedUnkPrefixByte);
                        }
                        else
                        {
                            dataArr[i] = ParseObjectDataBasic(reader, member.SubType & DataType.TYPE_MASK_BASIC_TYPES, data, parsedUnkPrefixByte);
                        }
                    }

                    memberArrays[mIdx] = dataArr;
                }

                if (bitValue == 0x80)
                {
                    byteIndex++;
                    bitValue = 1;
                }
                else
                {
                    bitValue <<= 1;
                }

                mIdx++;
            }
            while (dataMembers.Count > 0);

            object[] result = new object[structArrayLength];
            for (int i = 0; i < structArrayLength; i++)
            {
                ObjectData elem = new ObjectData { ClassName = meta.Name };
                for (int j = 0; j < memberNames.Length; j++)
                {
                    elem.MembersBitfield.Add(memberNames[j], memberPresent[j]);
                    if (memberPresent[j])
                    {
                        elem.MembersData.Add(memberNames[j], memberArrays[j][i]);
                    }
                }

                result[i] = elem;
            }

            return result;
        }
        else
        {
            ObjectData obj = new ObjectData
            {
                ClassName = meta.Name
            };

            do
            {
                var member = (Member)dataMembers.Dequeue()!;
                bool present = (dataBitfield[byteIndex] & bitValue) != 0;
                obj.MembersBitfield.Add(member.Name, present);

                if (present)
                {
                    obj.MembersData.Add(member.Name, ParseObjectData(reader, member, data));
                }

                if (bitValue == 0x80)
                {
                    byteIndex++;
                    bitValue = 1;
                }
                else
                {
                    bitValue <<= 1;
                }
            }
            while (dataMembers.Count > 0);

            return obj;
        }
    }

    private static object ParseObjectData(BinaryReader reader, Member member, BinaryTagfile data, byte unkPrefixByte = 8)
    {
        bool isArray = DataTypeIsArray(member.SubType);
        bool isTuple = DataTypeIsTuple(member.SubType);
        bool isStruct = DataTypeIsStruct(member.SubType);

        int arrayLength = 0;
        if (isArray)
        {
            var (pArrayLength, unk) = ParseVarInt(reader);
            arrayLength = (int)pArrayLength;
        }
        else if (isTuple)
        {
            arrayLength = member.CArraySize;
        }

        if ((isArray || isTuple) && arrayLength > 0)
        {
            if (isStruct)
            {
                var classMeta = data.Metadata.Select(m => m.Value).Where(v => v.Name.Equals(member.ClassName)).First() ?? throw new InvalidDataException($"Could not get metadata for ${member.ClassName}");

                if (arrayLength == 1 && classMeta.Members.Length == 1)
                {
                    return ParseObject(reader, classMeta, data);
                }
                else
                {
                    return ParseObject(reader, classMeta, data, true, arrayLength);
                }
            }
            else
            {
                byte parsedUnkPrefixByte = 8;
                if (DataTypeHasArrayPrefix(member.SubType))
                {
                    parsedUnkPrefixByte = reader.ReadByte();
                }

                object[] dataArr = new object[arrayLength];
                var basicSubType = member.SubType & DataType.TYPE_MASK_BASIC_TYPES;
                for (int i = 0; i < arrayLength; i++)
                {
                    dataArr[i] = ParseObjectDataBasic(reader, basicSubType, data, parsedUnkPrefixByte);
                }

                return dataArr;
            }
        }
        else if (isStruct)
        {
            var classMeta = data.Metadata.Select(m => m.Value).Where(v => v.Name.Equals(member.ClassName)).First();
            return classMeta == null
                ? throw new InvalidDataException($"Could not get meta for ${member.ClassName}")
                : ParseObject(reader, classMeta, data);
        }
        else
        {
            return ParseObjectDataBasic(reader, member.SubType, data, unkPrefixByte);
        }
    }

    private static object ParseObjectDataBasic(BinaryReader reader, DataType subtype, BinaryTagfile data, byte unkPrefixByte = 8)
    {
        switch (subtype)
        {
            case DataType.TYPE_BYTE:
                return reader.ReadByte();
            case DataType.TYPE_INT:
                return ParseVarInt(reader);
            case DataType.TYPE_REAL:
                return reader.ReadSingle();
            case DataType.TYPE_VEC_4:
                if (unkPrefixByte == 6)
                {
                    float a = reader.ReadSingle(), b = reader.ReadSingle(), c = reader.ReadSingle();
                    return (a, b, c, 0f);
                }
                else
                {
                    return (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }

            case DataType.TYPE_VEC_8:
                if (unkPrefixByte == 6)
                {
                    return new List<object>
                    {
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f)
                    };
                }
                else
                {
                    return new List<object>
                    {
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                    };
                }

            case DataType.TYPE_VEC_12:
                if (unkPrefixByte == 6)
                {
                    return new List<object>
                    {
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f)
                    };
                }
                else
                {
                    return new List<object>
                    {
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                    };
                }

            case DataType.TYPE_VEC_16:
                if (unkPrefixByte == 6)
                {
                    return new List<object>
                    {
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0f)
                    };
                }
                else
                {
                    return new List<object>
                    {
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                        (reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
                    };
                }

            case DataType.TYPE_OBJECT:
                var (objectRef, isInline) = ParseVarInt(reader);
                if (isInline)
                {
                    reader.ReadByte();
                    reader.ReadByte();
                    return ParseObjectInline(reader, data);
                }
                else
                {
                    return ObjectRefName(objectRef);
                }

            case DataType.TYPE_CSTRING:
                return ParseString(reader, data);

            default:
                throw new NotImplementedException($"Subtype {subtype}");
        }
    }

    private static Queue GetAllDataMembers(ClassData leaf)
    {
        Queue dataMembers = new Queue();
        Stack classHierarchy = new Stack();
        ClassData parentRef = leaf;
        do
        {
            classHierarchy.Push(parentRef);
            parentRef = parentRef.Parent!;
        }
        while (parentRef != null);

        do
        {
            ClassData classRef = (ClassData)classHierarchy.Pop()!;
            foreach (var member in classRef.Members)
            {
                dataMembers.Enqueue(member);
            }
        }
        while (classHierarchy.Count > 0);
        return dataMembers;
    }

    public class BinaryTagfile
    {
        public Dictionary<uint, string> Strings = [];
        public Dictionary<uint, ClassData> Metadata = [];
        public Dictionary<uint, ObjectData> Objects = [];
        public ObjectData? ObjectRoot;
        public byte Version;
    }

    public class ClassData
    {
        public string Name = string.Empty;
        public byte Version;
        public ClassData? Parent;
        public Member[] Members = [];
    }

    public class Member
    {
        public string Name = string.Empty;
        public DataType SubType;
        public string ClassName = string.Empty;
        public byte CArraySize;

        public override string ToString()
        {
            return $"{Name} ({SubType}{(CArraySize != 0 ? $" - [{CArraySize}]" : string.Empty)}{(ClassName != null ? $" - {ClassName}" : string.Empty)})";
        }
    }

    public class ObjectData
    {
        public uint BinIdx;
        public string ClassName = string.Empty;
        public Dictionary<string, bool> MembersBitfield = [];
        public Dictionary<string, object> MembersData = [];
    }
}
