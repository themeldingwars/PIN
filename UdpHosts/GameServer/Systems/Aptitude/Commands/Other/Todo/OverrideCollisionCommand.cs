using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class OverrideCollisionCommand : Command, ICommand
{
    private OverrideCollisionCommandDef Params;

    public OverrideCollisionCommand(OverrideCollisionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}