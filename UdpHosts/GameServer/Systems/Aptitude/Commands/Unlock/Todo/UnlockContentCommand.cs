using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockContentCommand : Command, ICommand
{
    private UnlockContentCommandDef Params;

    public UnlockContentCommand(UnlockContentCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}