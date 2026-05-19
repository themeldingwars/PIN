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

    public bool Execute(Context context)
    {
        return true;
    }
}