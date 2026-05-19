using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Effect;

public class RemovePermanentEffectCommand : Command, ICommand
{
    private RemovePermanentEffectCommandDef Params;

    public RemovePermanentEffectCommand(RemovePermanentEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}