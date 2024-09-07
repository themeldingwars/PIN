using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireBulletHitCommand : Command, ICommand
{
    private RequireBulletHitCommandDef Params;

    public RequireBulletHitCommand(RequireBulletHitCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}