using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequestArcJobsCommand : ICommand
{
    private RequestArcJobsCommandDef Params;

    public RequestArcJobsCommand(RequestArcJobsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}