using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockTitlesCommand : Command, ICommand
{
    private UnlockTitlesCommandDef Params;

    public UnlockTitlesCommand(UnlockTitlesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.TitleId == 0)
        {
            return true;
        }

        // todo aptitude: unlock title
        return true;
    }
}