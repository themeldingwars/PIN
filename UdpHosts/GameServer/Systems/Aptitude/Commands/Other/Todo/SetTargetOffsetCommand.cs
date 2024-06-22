using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class SetTargetOffsetCommand : ICommand
{
    private SetTargetOffsetCommandDef Params;

    public SetTargetOffsetCommand(SetTargetOffsetCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}