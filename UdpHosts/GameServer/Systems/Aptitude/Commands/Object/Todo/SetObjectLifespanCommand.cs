using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetObjectLifespanCommand : ICommand
{
    private SetObjectLifespanCommandDef Params;

    public SetObjectLifespanCommand(SetObjectLifespanCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}