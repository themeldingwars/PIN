using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyDamageByHeadshotCommand : ICommand
{
    private ModifyDamageByHeadshotCommandDef Params;

    public ModifyDamageByHeadshotCommand(ModifyDamageByHeadshotCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}