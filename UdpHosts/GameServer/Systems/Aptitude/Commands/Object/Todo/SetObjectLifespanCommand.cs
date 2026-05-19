using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Object;

public class SetObjectLifespanCommand : Command, ICommand
{
    private SetObjectLifespanCommandDef Params;

    public SetObjectLifespanCommand(SetObjectLifespanCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}