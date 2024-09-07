using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CreateAbilityObjectCommand : Command, ICommand
{
    private CreateAbilityObjectCommandDef Params;

    public CreateAbilityObjectCommand(CreateAbilityObjectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}