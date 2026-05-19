using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SinLinkRevealCommand : Command, ICommand
{
    private SinLinkRevealCommandDef Params;

    public SinLinkRevealCommand(SinLinkRevealCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        foreach (var target in context.Targets)
        {
            // todo: reveal for each target
        }

        result.SetPass();
        return;
    }
}