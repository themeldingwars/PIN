using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class InteractionInProgressCommand : Command, ICommand
{
    private InteractionInProgressCommandDef Params;

    public InteractionInProgressCommand(InteractionInProgressCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}