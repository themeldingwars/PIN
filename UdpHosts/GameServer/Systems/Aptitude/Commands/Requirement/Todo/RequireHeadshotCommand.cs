using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireHeadshotCommand : Command, ICommand
{
    private RequireHeadshotCommandDef Params;

    public RequireHeadshotCommand(RequireHeadshotCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}