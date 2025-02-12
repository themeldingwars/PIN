using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class SetGliderParametersCommand : Command, ICommand
{
    private SetGliderParametersCommandDef Params;

    public SetGliderParametersCommand(SetGliderParametersCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            if (Params.Value != null)
            {
                character.SetGliderProfileId((uint)Params.Value);
            }
        }

        return true;
    }
}