using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class CreateSpawnPointCommand : Command, ICommand
{
    private CreateSpawnPointCommandDef Params;

    public CreateSpawnPointCommand(CreateSpawnPointCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}