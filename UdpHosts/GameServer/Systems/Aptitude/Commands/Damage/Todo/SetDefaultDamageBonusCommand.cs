using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetDefaultDamageBonusCommand : ICommand
{
    private SetDefaultDamageBonusCommandDef Params;

    public SetDefaultDamageBonusCommand(SetDefaultDamageBonusCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}