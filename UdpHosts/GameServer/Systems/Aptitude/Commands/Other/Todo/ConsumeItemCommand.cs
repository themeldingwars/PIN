using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ConsumeItemCommand : Command, ICommand
{
    private ConsumeItemCommandDef Params;

    public ConsumeItemCommand(ConsumeItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}