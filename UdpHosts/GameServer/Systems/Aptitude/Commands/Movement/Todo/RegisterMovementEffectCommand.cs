using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

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