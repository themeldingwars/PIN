using GameServer.StaticDB.Records;

namespace GameServer.Systems.Aptitude.Commands;

public abstract class RequirementCommand : Command
{
    protected RequirementCommand(ICommandDef def)
        : base(def)
    {
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        Test(context, ref result);
    }

    public override void Reset(Context context)
    {
        return;
    }
}