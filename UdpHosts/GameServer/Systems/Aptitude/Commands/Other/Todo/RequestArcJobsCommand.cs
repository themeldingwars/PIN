using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RequestArcJobsCommand : Command, ICommand
{
    private RequestArcJobsCommandDef Params;

    public RequestArcJobsCommand(RequestArcJobsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}