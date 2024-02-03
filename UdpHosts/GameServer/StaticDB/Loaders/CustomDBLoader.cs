using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GameServer.Data.SDB.Records.customdata;
using Shared.Common;

public class CustomDBLoader
{
    private static readonly SnakeCasePropertyNamingPolicy Policy = new SnakeCasePropertyNamingPolicy();
    private readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = Policy,
        IncludeFields = true
    };

    public Dictionary<uint, AuthorizeTerminalCommandDef> LoadAuthorizeTerminalCommandDef() 
    {
        return LoadJSON<AuthorizeTerminalCommandDef>("./StaticDB/CustomData/aptgss_AuthorizeTerminalCommandDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, SetGliderParametersCommandDef> LoadSetGliderParametersCommandDef() 
    {
        return LoadJSON<SetGliderParametersCommandDef>("./StaticDB/CustomData/aptgss_agsSetGliderParametersDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ModifyPermissionCommandDef> LoadModifyPermissionCommandDef() 
    {
        return LoadJSON<ModifyPermissionCommandDef>("./StaticDB/CustomData/aptgss_agsModifyPermissionCommandDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, ImpactRemoveEffectCommandDef> LoadImpactRemoveEffectCommandDef() 
    {
        return LoadJSON<ImpactRemoveEffectCommandDef>("./StaticDB/CustomData/aptgss_agsImpactRemoveEffectCommandDef.json")
        .ToDictionary(row => row.Id);
    }

    public Dictionary<uint, Dictionary<uint, Melding>> LoadMelding() 
    {
        return LoadJSON<Melding>("./StaticDB/CustomData/melding.json")
        .GroupBy(row => row.ZoneId)
        .ToDictionary(group => group.Key, group => group.ToDictionary(row => row.Id, row => row));
    }

    public Dictionary<uint, Dictionary<uint, Outpost>> LoadOutpost() 
    {
        return LoadJSON<Outpost>("./StaticDB/CustomData/outpost.json")
        .GroupBy(row => row.ZoneId)
        .ToDictionary(group => group.Key, group => group.ToDictionary(row => row.Id, row => row));
    }

    private T[] LoadJSON<T>(string fileName)
    {
        string jsonString = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<T[]>(jsonString, SerializerOptions);
    }
}
