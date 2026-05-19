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

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.TitleId == 0)
        {
            result.SetPass();
            return;
        }

        // todo aptitude: unlock title
        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}