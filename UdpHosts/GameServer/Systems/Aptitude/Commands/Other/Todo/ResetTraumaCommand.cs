using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ResetTraumaCommand : Command, ICommand
{
    private ResetTraumaCommandDef Params;

    public ResetTraumaCommand(ResetTraumaCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}