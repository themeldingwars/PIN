using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class TurretControlCommand : Command, ICommand
{
    private TurretControlCommandDef Params;

    public TurretControlCommand(TurretControlCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}