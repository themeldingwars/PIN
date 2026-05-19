using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Effect;

public class RemoveEffectByTagCommand : Command, ICommand
{
    private RemoveEffectByTagCommandDef Params;

    public RemoveEffectByTagCommand(RemoveEffectByTagCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}