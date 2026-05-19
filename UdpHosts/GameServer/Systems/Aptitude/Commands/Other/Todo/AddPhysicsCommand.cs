using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class AddPhysicsCommand : Command, ICommand
{
    private AddPhysicsCommandDef Params;

    public AddPhysicsCommand(AddPhysicsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}