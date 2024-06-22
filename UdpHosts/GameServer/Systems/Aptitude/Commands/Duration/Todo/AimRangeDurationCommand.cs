using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class AimRangeDurationCommand : ICommand
{
    private AimRangeDurationCommandDef Params;

    public AimRangeDurationCommand(AimRangeDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}