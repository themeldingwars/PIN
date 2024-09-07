using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SpawnLootCommand : Command, ICommand
{
    private SpawnLootCommandDef Params;

    public SpawnLootCommand(SpawnLootCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}