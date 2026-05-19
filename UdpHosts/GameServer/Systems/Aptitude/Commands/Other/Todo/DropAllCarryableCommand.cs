using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class DropAllCarryableCommand : Command, ICommand
{
    private DropAllCarryableCommandDef Params;

    public DropAllCarryableCommand(DropAllCarryableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}