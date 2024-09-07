using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyHostilityCommand : Command, ICommand
{
    private ModifyHostilityCommandDef Params;

    public ModifyHostilityCommand(ModifyHostilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}