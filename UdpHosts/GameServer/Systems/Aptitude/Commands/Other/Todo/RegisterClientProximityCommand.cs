using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RegisterClientProximityCommand : ICommand
{
    private RegisterClientProximityCommandDef Params;

    public RegisterClientProximityCommand(RegisterClientProximityCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}