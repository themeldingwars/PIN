using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Update;

// TODO: UpdateWaitCommand
public class UpdateWaitCommand : Command, ICommand
{
    private UpdateWaitCommandDef Params;

    public UpdateWaitCommand(UpdateWaitCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}