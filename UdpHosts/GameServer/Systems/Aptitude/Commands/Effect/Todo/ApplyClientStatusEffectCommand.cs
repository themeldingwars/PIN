using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Effect;

public class ApplyClientStatusEffectCommand : Command, ICommand
{
    private ApplyClientStatusEffectCommandDef Params;

    public ApplyClientStatusEffectCommand(ApplyClientStatusEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}