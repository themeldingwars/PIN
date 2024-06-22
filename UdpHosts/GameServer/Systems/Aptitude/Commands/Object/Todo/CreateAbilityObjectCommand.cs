using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class CreateAbilityObjectCommand : ICommand
{
    private CreateAbilityObjectCommandDef Params;

    public CreateAbilityObjectCommand(CreateAbilityObjectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}