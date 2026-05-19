using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

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