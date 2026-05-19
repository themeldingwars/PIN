using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockCertsCommand : Command, ICommand
{
    private UnlockCertsCommandDef Params;

    public UnlockCertsCommand(UnlockCertsCommandDef par)
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