using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Update;

public class UpdateWaitCommand : Command, ICommand
{
    private UpdateWaitCommandDef Params;

    public UpdateWaitCommand(UpdateWaitCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}