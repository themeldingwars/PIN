using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AddLootTableCommand : Command, ICommand
{
    private AddLootTableCommandDef Params;

    public AddLootTableCommand(AddLootTableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}