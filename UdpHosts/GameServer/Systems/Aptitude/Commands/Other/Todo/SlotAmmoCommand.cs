using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SlotAmmoCommand : Command, ICommand
{
    private SlotAmmoCommandDef Params;

    public SlotAmmoCommand(SlotAmmoCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}