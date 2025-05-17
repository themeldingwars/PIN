using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class ReturnCommand : Command, ICommand
{
    private ReturnCommandDef Params;

    public ReturnCommand(ReturnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // return status: 3
        // 30669. Terrorclaw "Pound" Melee TEST ; Shadowstrike as a Beetle "pound the ground" attack.
        return true;
    }
}