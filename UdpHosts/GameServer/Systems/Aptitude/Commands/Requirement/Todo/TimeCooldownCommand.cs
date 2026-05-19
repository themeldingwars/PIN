using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class TimeCooldownCommand : Command, ICommand
{
    private TimeCooldownCommandDef Params;

    public TimeCooldownCommand(TimeCooldownCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}