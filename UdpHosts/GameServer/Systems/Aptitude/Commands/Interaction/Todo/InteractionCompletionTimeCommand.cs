using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class InteractionCompletionTimeCommand : Command, ICommand
{
    private InteractionCompletionTimeCommandDef Params;

    public InteractionCompletionTimeCommand(InteractionCompletionTimeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}