using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

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