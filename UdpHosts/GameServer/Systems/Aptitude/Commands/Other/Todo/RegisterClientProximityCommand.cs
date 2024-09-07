using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RegisterClientProximityCommand : Command, ICommand
{
    private RegisterClientProximityCommandDef Params;

    public RegisterClientProximityCommand(RegisterClientProximityCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}