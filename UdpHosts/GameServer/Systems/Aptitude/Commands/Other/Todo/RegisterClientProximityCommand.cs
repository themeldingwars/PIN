using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

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