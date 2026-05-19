using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.TinyObject;

public class TinyObjectUpdateCommand : Command, ICommand
{
    private TinyObjectUpdateCommandDef Params;

    public TinyObjectUpdateCommand(TinyObjectUpdateCommandDef par)
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