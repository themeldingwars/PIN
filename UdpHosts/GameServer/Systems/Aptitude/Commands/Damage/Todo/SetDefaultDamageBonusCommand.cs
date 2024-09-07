using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetDefaultDamageBonusCommand : Command, ICommand
{
    private SetDefaultDamageBonusCommandDef Params;

    public SetDefaultDamageBonusCommand(SetDefaultDamageBonusCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}