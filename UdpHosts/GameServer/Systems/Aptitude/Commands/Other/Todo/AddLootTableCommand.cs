using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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