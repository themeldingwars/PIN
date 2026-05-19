using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RegisterTimedTriggerCommand : Command, ICommand
{
    private RegisterTimedTriggerCommandDef Params;

    public RegisterTimedTriggerCommand(RegisterTimedTriggerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}