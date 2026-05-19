using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Effect;

public class RemoveClientStatusEffectCommand : Command, ICommand
{
    private RemoveClientStatusEffectCommandDef Params;

    public RemoveClientStatusEffectCommand(RemoveClientStatusEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}