using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SetHoverParametersCommand : Command, ICommand
{
    private SetHoverParametersCommandDef Params;

    public SetHoverParametersCommand(SetHoverParametersCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            // if (Params.Value != null)
            // {
            //     character.SetHoverProfileId(Params.Value);
            // }
        }

        return true;
    }
}