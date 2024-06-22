using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class BullrushCommand : ICommand
{
    private BullrushCommandDef Params;

    public BullrushCommand(BullrushCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}