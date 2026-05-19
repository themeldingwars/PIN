using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SinAcquireCommand : Command, ICommand
{
    private SinAcquireCommandDef Params;

    public SinAcquireCommand(SinAcquireCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}