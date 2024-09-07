using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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