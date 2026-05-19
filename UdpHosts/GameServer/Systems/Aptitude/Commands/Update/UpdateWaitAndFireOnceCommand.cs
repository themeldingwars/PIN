using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Update;

// TODO: UpdateWaitAndFireOnceCommand
public class UpdateWaitAndFireOnceCommand : Command, ICommand
{
    private UpdateWaitAndFireOnceCommandDef Params;

    public UpdateWaitAndFireOnceCommand(UpdateWaitAndFireOnceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.Chain);
        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}