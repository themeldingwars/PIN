using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class AddAppendageHealthPoolCommand : Command, ICommand
{
    private AddAppendageHealthPoolCommandDef Params;

    public AddAppendageHealthPoolCommand(AddAppendageHealthPoolCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}