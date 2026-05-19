using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class SetInteractionTypeCommand : Command, ICommand
{
    private SetInteractionTypeCommandDef Params;

    public SetInteractionTypeCommand(SetInteractionTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}