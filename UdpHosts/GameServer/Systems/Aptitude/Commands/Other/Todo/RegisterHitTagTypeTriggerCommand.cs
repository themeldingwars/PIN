using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RegisterHitTagTypeTriggerCommand : Command, ICommand
{
    private RegisterHitTagTypeTriggerCommandDef Params;

    public RegisterHitTagTypeTriggerCommand(RegisterHitTagTypeTriggerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}