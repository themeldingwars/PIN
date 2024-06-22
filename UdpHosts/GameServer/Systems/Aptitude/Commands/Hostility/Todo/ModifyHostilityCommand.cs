using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyHostilityCommand : ICommand
{
    private ModifyHostilityCommandDef Params;

    public ModifyHostilityCommand(ModifyHostilityCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}