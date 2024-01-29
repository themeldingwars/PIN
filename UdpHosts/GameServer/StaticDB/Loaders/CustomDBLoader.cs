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

    private T[] LoadJSON<T>(string fileName)
    {
        string jsonString = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<T[]>(jsonString, SerializerOptions);
    }
}
