using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RestockAmmoCommand : ICommand
{
    private RestockAmmoCommandDef Params;

    public RestockAmmoCommand(RestockAmmoCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}