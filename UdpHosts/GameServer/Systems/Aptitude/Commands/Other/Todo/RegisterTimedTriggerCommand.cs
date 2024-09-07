using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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