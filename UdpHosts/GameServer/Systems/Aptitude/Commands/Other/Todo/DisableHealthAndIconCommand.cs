using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class DisableHealthAndIconCommand : Command, ICommand
{
    private DisableHealthAndIconCommandDef Params;

    public DisableHealthAndIconCommand(DisableHealthAndIconCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}