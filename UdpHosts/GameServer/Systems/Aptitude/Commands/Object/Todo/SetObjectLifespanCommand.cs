using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Object;

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