using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class SlotAmmoCommand : ICommand
{
    private SlotAmmoCommandDef Params;

    public SlotAmmoCommand(SlotAmmoCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}