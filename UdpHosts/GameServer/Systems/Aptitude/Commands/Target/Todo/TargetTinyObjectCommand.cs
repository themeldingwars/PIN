using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetTinyObjectCommand : ICommand
{
    private TargetTinyObjectCommandDef Params;

    public TargetTinyObjectCommand(TargetTinyObjectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}