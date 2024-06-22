using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RegisterHitTagTypeTriggerCommand : ICommand
{
    private RegisterHitTagTypeTriggerCommandDef Params;

    public RegisterHitTagTypeTriggerCommand(RegisterHitTagTypeTriggerCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}