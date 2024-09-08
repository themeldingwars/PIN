using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TemporaryEquipmentCommand : Command, ICommand
{
    private TemporaryEquipmentCommandDef Params;

    public TemporaryEquipmentCommand(TemporaryEquipmentCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // based on ff1189 should act on context.Self
        return true;
    }
}