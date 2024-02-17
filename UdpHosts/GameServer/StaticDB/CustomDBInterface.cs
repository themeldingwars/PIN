namespace GameServer.Data.SDB;

using System.Collections.Generic;
using Records.customdata;

public class CustomDBInterface
{
    // aptgss
    private static Dictionary<uint, AuthorizeTerminalCommandDef> AuthorizeTerminalCommandDef;
    private static Dictionary<uint, SetGliderParametersCommandDef> SetGliderParametersCommandDef;
    private static Dictionary<uint, ModifyPermissionCommandDef> ModifyPermissionCommandDef;
    private static Dictionary<uint, ImpactRemoveEffectCommandDef> ImpactRemoveEffectCommandDef;

    // custom
    private static Dictionary<uint, Dictionary<uint, Deployable>> Deployable;
    private static Dictionary<uint, Dictionary<uint, Melding>> Melding;
    private static Dictionary<uint, Dictionary<uint, Outpost>> Outpost;

    public static void Init()
    {
        var loader = new CustomDBLoader();

        // aptgss
        AuthorizeTerminalCommandDef = loader.LoadAuthorizeTerminalCommandDef();
        SetGliderParametersCommandDef = loader.LoadSetGliderParametersCommandDef();
        ModifyPermissionCommandDef = loader.LoadModifyPermissionCommandDef();
        ImpactRemoveEffectCommandDef = loader.LoadImpactRemoveEffectCommandDef();

        // custom
        Deployable = loader.LoadDeployable();
        Melding = loader.LoadMelding();
        Outpost = loader.LoadOutpost();
    }

    // aptgss
    public static AuthorizeTerminalCommandDef GetAuthorizeTerminalCommandDef(uint id) => AuthorizeTerminalCommandDef.GetValueOrDefault(id);
    public static SetGliderParametersCommandDef GetSetGliderParametersCommandDef(uint id) => SetGliderParametersCommandDef.GetValueOrDefault(id);
    public static ModifyPermissionCommandDef GetModifyPermissionCommandDef(uint id) => ModifyPermissionCommandDef.GetValueOrDefault(id);
    public static ImpactRemoveEffectCommandDef GetImpactRemoveEffectCommandDef(uint id) => ImpactRemoveEffectCommandDef.GetValueOrDefault(id);

    // custom
    public static Dictionary<uint, Deployable> GetZoneDeployables(uint zoneId) => Deployable.GetValueOrDefault(zoneId);
    public static Dictionary<uint, Melding> GetZoneMeldings(uint zoneId) => Melding.GetValueOrDefault(zoneId);
    public static Dictionary<uint, Outpost> GetZoneOutposts(uint zoneId) => Outpost.GetValueOrDefault(zoneId);
}