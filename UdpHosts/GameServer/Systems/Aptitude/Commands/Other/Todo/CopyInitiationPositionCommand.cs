using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class CopyInitiationPositionCommand : Command, ICommand
{
    private CopyInitiationPositionCommandDef Params;

    public CopyInitiationPositionCommand(CopyInitiationPositionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}