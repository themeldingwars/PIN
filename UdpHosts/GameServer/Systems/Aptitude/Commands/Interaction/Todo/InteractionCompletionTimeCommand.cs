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

    public bool Execute(Context context)
    {
        return true;
    }
}