using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class EquipLoadoutCommand : Command, ICommand
{
    private EquipLoadoutCommandDef Params;

    public EquipLoadoutCommand(EquipLoadoutCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}