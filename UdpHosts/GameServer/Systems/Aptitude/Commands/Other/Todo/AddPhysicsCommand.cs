using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class AddPhysicsCommand : ICommand
{
    private AddPhysicsCommandDef Params;

    public AddPhysicsCommand(AddPhysicsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}