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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}