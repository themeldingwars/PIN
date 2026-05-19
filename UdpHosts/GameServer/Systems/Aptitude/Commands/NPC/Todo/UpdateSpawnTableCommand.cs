using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

public class UpdateSpawnTableCommand : Command, ICommand
{
    private UpdateSpawnTableCommandDef Params;

    public UpdateSpawnTableCommand(UpdateSpawnTableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}