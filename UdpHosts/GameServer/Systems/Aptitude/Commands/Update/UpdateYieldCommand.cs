using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Update;

// TODO: UpdateYieldCommand
public class UpdateYieldCommand : Command, ICommand
{
    private UpdateYieldCommandDef Params;

    public UpdateYieldCommand(UpdateYieldCommandDef par)
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