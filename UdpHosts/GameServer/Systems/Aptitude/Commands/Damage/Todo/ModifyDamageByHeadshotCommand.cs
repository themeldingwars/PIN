using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByHeadshotCommand : Command, ICommand
{
    private ModifyDamageByHeadshotCommandDef Params;

    public ModifyDamageByHeadshotCommand(ModifyDamageByHeadshotCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}