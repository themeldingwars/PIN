using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

public class TemporaryEquipmentCommand : Command, ICommand
{
    private TemporaryEquipmentCommandDef Params;

    public TemporaryEquipmentCommand(TemporaryEquipmentCommandDef par)
: base(par)
    {
        Params = par;
    }

    // based on ff1189 should act on context.Self
    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}