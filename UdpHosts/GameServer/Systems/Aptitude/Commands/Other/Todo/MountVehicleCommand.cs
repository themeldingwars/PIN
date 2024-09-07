using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class MountVehicleCommand : Command, ICommand
{
    private MountVehicleCommandDef Params;

    public MountVehicleCommand(MountVehicleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}