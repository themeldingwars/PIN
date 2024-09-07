using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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