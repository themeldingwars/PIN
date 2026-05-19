using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Object;

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