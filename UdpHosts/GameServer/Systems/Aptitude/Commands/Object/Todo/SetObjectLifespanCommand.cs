using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetObjectLifespanCommand : Command, ICommand
{
    private SetObjectLifespanCommandDef Params;

    public SetObjectLifespanCommand(SetObjectLifespanCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}