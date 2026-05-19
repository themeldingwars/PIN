using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Hostility;

public class SetHostilityCommand : Command, ICommand
{
    private SetHostilityCommandDef Params;

    public SetHostilityCommand(SetHostilityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}