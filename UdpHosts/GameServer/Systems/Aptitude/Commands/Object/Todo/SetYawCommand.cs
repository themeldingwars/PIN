using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetYawCommand : ICommand
{
    private SetYawCommandDef Params;

    public SetYawCommand(SetYawCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}