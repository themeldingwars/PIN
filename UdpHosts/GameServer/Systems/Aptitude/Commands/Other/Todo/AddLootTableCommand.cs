using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AddLootTableCommand : ICommand
{
    private AddLootTableCommandDef Params;

    public AddLootTableCommand(AddLootTableCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}