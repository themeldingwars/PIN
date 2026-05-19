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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}