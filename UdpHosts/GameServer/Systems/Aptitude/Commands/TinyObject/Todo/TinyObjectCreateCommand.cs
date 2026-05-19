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

    public bool Execute(Context context)
    {
        return true;
    }
}