using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SetVisualInfoIndexCommand : Command, ICommand
{
    private SetVisualInfoIndexCommandDef Params;

    public SetVisualInfoIndexCommand(SetVisualInfoIndexCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}