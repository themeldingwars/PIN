using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RegisterMovementEffectCommand : ICommand
{
    private RegisterMovementEffectCommandDef Params;

    public RegisterMovementEffectCommand(RegisterMovementEffectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}