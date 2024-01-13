namespace GameServer.Data.SDB;

using System;
using System.Collections.Generic;
using CommandLine;
using FauFau.Formats;
using Records.dbitems;
using Records.dbviusalrecords;
using Serilog.Events;

public class SDBInterface
{
    private static Dictionary<uint, WarpaintPalette> WarpaintPalettes;
    private static Dictionary<uint, AttributeCategory> AttributeCategory;
    private static Dictionary<uint, AttributeDefinition> AttributeDefinition;
    private static Dictionary<KeyValuePair<uint, ushort>, AttributeRange> AttributeRange;
    private static Dictionary<KeyValuePair<uint, ushort>, ItemCharacterScalars> ItemCharacterScalars;
    private static Dictionary<uint, RootItem> RootItem;

    public static void Init(StaticDB instance)
    {
        var loader = new StaticDBLoader(instance);
        WarpaintPalettes = loader.LoadWarpaintPalettes();
        AttributeCategory = loader.LoadAttributeCategory();
        AttributeDefinition = loader.LoadAttributeDefinition();
        AttributeRange = loader.LoadAttributeRange();
        ItemCharacterScalars = loader.LoadItemCharacterScalars();
        RootItem = loader.LoadRootItem();
        // Console.WriteLine($"Testing WarpaintPalettes: {GetWarpaintPalette(76714u)}");
    }

    public static WarpaintPalette GetWarpaintPalette(uint id) => WarpaintPalettes.GetValueOrDefault(id);
}