using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetStackEmptyCommand : Command, ICommand
{
    private TargetStackEmptyCommandDef Params;

    public TargetStackEmptyCommand(TargetStackEmptyCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.NotEmpty == 1)
        {
            return context.Targets.Count != 0;
        }

        return context.Targets.Count == 0;
    }
}