using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHasCertificateCommand : Command, ICommand
{
    private RequireHasCertificateCommandDef Params;

    public RequireHasCertificateCommand(RequireHasCertificateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}