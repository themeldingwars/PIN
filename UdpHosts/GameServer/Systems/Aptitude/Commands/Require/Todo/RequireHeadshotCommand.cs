using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RequireHeadshotCommand : ICommand
{
    private RequireHeadshotCommandDef Params;

    public RequireHeadshotCommand(RequireHeadshotCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}