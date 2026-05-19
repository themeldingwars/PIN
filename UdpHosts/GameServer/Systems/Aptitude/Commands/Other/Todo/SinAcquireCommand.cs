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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}