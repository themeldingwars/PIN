using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireBulletHitCommand : ICommand
{
    private RequireBulletHitCommandDef Params;

    public RequireBulletHitCommand(RequireBulletHitCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}