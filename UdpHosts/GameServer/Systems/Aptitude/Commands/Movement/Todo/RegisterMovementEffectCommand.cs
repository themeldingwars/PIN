using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class RegisterMovementEffectCommand : Command, ICommand
{
    private RegisterMovementEffectCommandDef Params;

    public RegisterMovementEffectCommand(RegisterMovementEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}