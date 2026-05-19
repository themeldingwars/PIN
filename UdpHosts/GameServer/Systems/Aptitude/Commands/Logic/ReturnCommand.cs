using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class ReturnCommand : Command, ICommand
{
    private ReturnCommandDef Params;

    public ReturnCommand(ReturnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // return status: 3
        // 30669. Terrorclaw "Pound" Melee TEST ; Shadowstrike as a Beetle "pound the ground" attack.

        // TODO:
    }
}