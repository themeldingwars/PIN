using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class TemporaryEquipmentStatMappingCommand : Command, ICommand
{
    private TemporaryEquipmentStatMappingCommandDef Params;

    public TemporaryEquipmentStatMappingCommand(TemporaryEquipmentStatMappingCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}