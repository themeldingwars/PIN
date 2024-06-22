using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class MountVehicleCommand : ICommand
{
    private MountVehicleCommandDef Params;

    public MountVehicleCommand(MountVehicleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}