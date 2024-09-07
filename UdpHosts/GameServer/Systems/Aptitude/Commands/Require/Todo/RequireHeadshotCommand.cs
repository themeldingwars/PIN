using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHeadshotCommand : Command, ICommand
{
    private RequireHeadshotCommandDef Params;

    public RequireHeadshotCommand(RequireHeadshotCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}