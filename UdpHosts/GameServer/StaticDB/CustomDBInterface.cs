namespace GameServer.Data.SDB;

using System.Collections.Generic;
using Records.customdata;

public class CustomDBInterface
{
    // aptgss
    private static Dictionary<uint, AuthorizeTerminalCommandDef> AuthorizeTerminalCommandDef;
    private static Dictionary<uint, SetGliderParametersCommandDef> SetGliderParametersCommandDef;
    private static Dictionary<uint, ModifyPermissionCommandDef> ModifyPermissionCommandDef;

    public static void Init()
    {
        var loader = new CustomDBLoader();

        // aptgss
        AuthorizeTerminalCommandDef = loader.LoadAuthorizeTerminalCommandDef();
        SetGliderParametersCommandDef = loader.LoadSetGliderParametersCommandDef();
        ModifyPermissionCommandDef = loader.LoadModifyPermissionCommandDef();
    }

    // aptgss
    public static AuthorizeTerminalCommandDef GetAuthorizeTerminalCommandDef(uint id) => AuthorizeTerminalCommandDef.GetValueOrDefault(id);
    public static SetGliderParametersCommandDef GetSetGliderParametersCommandDef(uint id) => SetGliderParametersCommandDef.GetValueOrDefault(id);
    public static ModifyPermissionCommandDef GetModifyPermissionCommandDef(uint id) => ModifyPermissionCommandDef.GetValueOrDefault(id);
}