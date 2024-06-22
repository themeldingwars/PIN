using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EquipLoadoutCommand : ICommand
{
    private EquipLoadoutCommandDef Params;

    public EquipLoadoutCommand(EquipLoadoutCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}