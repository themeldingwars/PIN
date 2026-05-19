using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class FireProjectileCommand : Command, ICommand
{
    private FireProjectileCommandDef Params;

    public FireProjectileCommand(FireProjectileCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}