using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.SetFlags;

public class CombatFlagsCommand : Command, ICommand
{
    private CombatFlagsCommandDef Params;

    public CombatFlagsCommand(CombatFlagsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}