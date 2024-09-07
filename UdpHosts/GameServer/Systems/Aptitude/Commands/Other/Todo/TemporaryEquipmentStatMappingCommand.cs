using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TemporaryEquipmentStatMappingCommand : Command, ICommand
{
    private TemporaryEquipmentStatMappingCommandDef Params;

    public TemporaryEquipmentStatMappingCommand(TemporaryEquipmentStatMappingCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}