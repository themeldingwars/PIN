using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TemporaryEquipmentStatMappingCommand : ICommand
{
    private TemporaryEquipmentStatMappingCommandDef Params;

    public TemporaryEquipmentStatMappingCommand(TemporaryEquipmentStatMappingCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}