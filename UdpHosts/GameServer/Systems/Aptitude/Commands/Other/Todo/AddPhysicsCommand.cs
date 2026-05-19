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

    public bool Execute(Context context)
    {
        return true;
    }
}