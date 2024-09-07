using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EquipLoadoutCommand : Command, ICommand
{
    private EquipLoadoutCommandDef Params;

    public EquipLoadoutCommand(EquipLoadoutCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}