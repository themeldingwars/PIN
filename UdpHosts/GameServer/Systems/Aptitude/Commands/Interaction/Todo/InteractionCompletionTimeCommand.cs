using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class InteractionCompletionTimeCommand : ICommand
{
    private InteractionCompletionTimeCommandDef Params;

    public InteractionCompletionTimeCommand(InteractionCompletionTimeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}