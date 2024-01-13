namespace GameServer.Data.SDB;

using FauFau.Formats;
using Records.dbitems;
using Records.dbviusalrecords;
using System;
using System.Collections.Generic;
using System.Linq;
using static FauFau.Formats.StaticDB;
using Shared.Common;

public class StaticDBLoader : ISDBLoader
{
    private static readonly SnakeCasePropertyNamingPolicy Policy = new SnakeCasePropertyNamingPolicy();
    private static StaticDB sdb;

    public StaticDBLoader(StaticDB instance)
    {
        sdb = instance;
    }

    public Dictionary<uint, WarpaintPalette> LoadWarpaintPalettes() 
    {
        return LoadStaticDB<WarpaintPalette>("dbvisualrecords::WarpaintPalette")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AttributeCategory> LoadAttributeCategory() 
    {
        return LoadStaticDB<AttributeCategory>("dbitems::AttributeCategory")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, AttributeDefinition> LoadAttributeDefinition() 
    {
        return LoadStaticDB<AttributeDefinition>("dbitems::AttributeDefinition")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<KeyValuePair<uint, ushort>, AttributeRange> LoadAttributeRange() 
    {
        // There are duplicates, like item 78084 which has the range attribute twice. Ingame, it seems too use only one result for that one, so chosing to do the same here.
        return LoadStaticDB<AttributeRange>("dbitems::AttributeRange")
        .GroupBy(row => new KeyValuePair<uint, ushort>(row.ItemId, row.AttributeId))
        .ToDictionary(group => group.Key, group => group.Last());
    }
    
    public Dictionary<KeyValuePair<uint, ushort>, ItemCharacterScalars> LoadItemCharacterScalars() 
    {
        return LoadStaticDB<ItemCharacterScalars>("dbitems::ItemCharacterScalars")
        .ToDictionary(row => new KeyValuePair<uint, ushort>(row.ItemId, row.AttributeCategory));
    }

    public Dictionary<uint, RootItem> LoadRootItem() 
    {
        return LoadStaticDB<RootItem>("dbitems::RootItem")
        .ToDictionary(row => row.SdbId);
    }

    private static T[] LoadStaticDB<T>(string tableName)
    where T : class, new()
    {
        Table table = sdb.GetTableByName(tableName);
        var list = new List<T>();
        foreach(Row row in table.Rows)
        {
            T entry = new T();
            var propInfoList = entry.GetType().GetProperties().ToList();
            foreach(var propInfo in propInfoList)
            {
                string convertedName = Policy.ConvertName(propInfo.Name);
                int index = table.GetColumnIndexByName(convertedName);
                if (index != -1)
                {
                    propInfo.SetValue(entry, row[index], null);
                }
                else
                {
                    Console.WriteLine($"Could not find column for {propInfo.Name} (converted to {convertedName})");
                }
            }

            list.Add(entry);
        }

        return list.ToArray();
    }
}