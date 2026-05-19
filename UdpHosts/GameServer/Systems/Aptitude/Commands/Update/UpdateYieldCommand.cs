using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Update;

public class UpdateYieldCommand : Command, ICommand
{
    private UpdateYieldCommandDef Params;

    public UpdateYieldCommand(UpdateYieldCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}