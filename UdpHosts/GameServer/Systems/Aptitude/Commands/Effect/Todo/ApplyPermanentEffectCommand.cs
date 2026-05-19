using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Effect;

public class ApplyPermanentEffectCommand : Command, ICommand
{
    private ApplyPermanentEffectCommandDef Params;

    public ApplyPermanentEffectCommand(ApplyPermanentEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}