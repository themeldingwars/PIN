using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Hostility;

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