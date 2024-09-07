using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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