using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CalldownVehicleCommand : Command, ICommand
{
    private CalldownVehicleCommandDef Params;

    public CalldownVehicleCommand(CalldownVehicleCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}