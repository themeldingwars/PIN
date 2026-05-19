using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.TinyObject;

public class TinyObjectDestroyCommand : Command, ICommand
{
    private TinyObjectDestroyCommandDef Params;

    public TinyObjectDestroyCommand(TinyObjectDestroyCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}