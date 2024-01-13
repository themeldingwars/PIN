namespace GameServer.Data.SDB;

using Records.dbviusalrecords;
using Records.dbitems;
using System.Collections.Generic;

public interface ISDBLoader
{
    Dictionary<uint, WarpaintPalette> LoadWarpaintPalettes();
    Dictionary<uint, AttributeCategory> LoadAttributeCategory();
    Dictionary<uint, AttributeDefinition> LoadAttributeDefinition();
    Dictionary<KeyValuePair<uint, ushort>, AttributeRange> LoadAttributeRange();
    Dictionary<KeyValuePair<uint, ushort>, ItemCharacterScalars> LoadItemCharacterScalars();
    Dictionary<uint, RootItem> LoadRootItem();
}