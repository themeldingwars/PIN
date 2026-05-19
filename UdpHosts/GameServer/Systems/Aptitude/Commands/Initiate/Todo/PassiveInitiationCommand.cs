using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Initiate;

public class PassiveInitiationCommand : Command, ICommand
{
    private PassiveInitiationCommandDef Params;

    public PassiveInitiationCommand(PassiveInitiationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}