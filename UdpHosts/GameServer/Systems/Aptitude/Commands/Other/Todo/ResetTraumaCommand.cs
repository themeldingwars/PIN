using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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