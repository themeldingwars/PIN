using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RequestAbilitySelectionCommand : Command, ICommand
{
    private RequestAbilitySelectionCommandDef Params;

    public RequestAbilitySelectionCommand(RequestAbilitySelectionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}