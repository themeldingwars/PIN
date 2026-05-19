using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class BullrushCommand : Command, ICommand
{
    private BullrushCommandDef Params;

    public BullrushCommand(BullrushCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}