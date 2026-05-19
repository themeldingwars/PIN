using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class MindControlCommand : Command, ICommand
{
    private MindControlCommandDef Params;

    public MindControlCommand(MindControlCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}