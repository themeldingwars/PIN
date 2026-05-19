using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Damage;

public class BombardmentCommand : Command, ICommand
{
    private BombardmentCommandDef Params;

    public BombardmentCommand(BombardmentCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}