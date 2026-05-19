using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

public class ActivateSpawnTableCommand : Command, ICommand
{
    private ActivateSpawnTableCommandDef Params;

    public ActivateSpawnTableCommand(ActivateSpawnTableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}