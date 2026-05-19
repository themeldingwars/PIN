using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SpawnLootCommand : Command, ICommand
{
    private SpawnLootCommandDef Params;

    public SpawnLootCommand(SpawnLootCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}