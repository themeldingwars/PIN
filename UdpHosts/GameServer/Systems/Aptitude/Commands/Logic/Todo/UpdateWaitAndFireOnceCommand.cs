using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class UpdateWaitAndFireOnceCommand : Command, ICommand
{
    private UpdateWaitAndFireOnceCommandDef Params;

    public UpdateWaitAndFireOnceCommand(UpdateWaitAndFireOnceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.Chain);

        return true;
    }
}