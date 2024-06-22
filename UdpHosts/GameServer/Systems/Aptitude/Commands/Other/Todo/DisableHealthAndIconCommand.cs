using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class DisableHealthAndIconCommand : ICommand
{
    private DisableHealthAndIconCommandDef Params;

    public DisableHealthAndIconCommand(DisableHealthAndIconCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}