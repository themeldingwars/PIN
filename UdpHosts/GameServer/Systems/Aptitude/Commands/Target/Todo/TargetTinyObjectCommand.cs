using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TargetTinyObjectCommand : Command, ICommand
{
    private TargetTinyObjectCommandDef Params;

    public TargetTinyObjectCommand(TargetTinyObjectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}