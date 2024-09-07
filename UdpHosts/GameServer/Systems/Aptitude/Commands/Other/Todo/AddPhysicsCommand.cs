using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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