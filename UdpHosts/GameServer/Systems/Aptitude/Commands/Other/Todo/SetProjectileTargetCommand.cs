using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class SetProjectileTargetCommand : Command, ICommand
{
    private SetProjectileTargetCommandDef Params;

    public SetProjectileTargetCommand(SetProjectileTargetCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}