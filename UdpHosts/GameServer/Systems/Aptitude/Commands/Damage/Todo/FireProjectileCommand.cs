using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class FireProjectileCommand : ICommand
{
    private FireProjectileCommandDef Params;

    public FireProjectileCommand(FireProjectileCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}