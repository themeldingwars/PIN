using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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