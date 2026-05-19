using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.TinyObject;

public class TinyObjectCreateCommand : Command, ICommand
{
    private TinyObjectCreateCommandDef Params;

    public TinyObjectCreateCommand(TinyObjectCreateCommandDef par)
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