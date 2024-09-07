using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class AimRangeDurationCommand : Command, ICommand
{
    private AimRangeDurationCommandDef Params;

    public AimRangeDurationCommand(AimRangeDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}