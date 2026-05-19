using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class SetGliderParametersCommand : Command, ICommand
{
    private SetGliderParametersCommandDef Params;

    public SetGliderParametersCommand(SetGliderParametersCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            if (Params.Value != null)
            {
                character.SetGliderProfileId((uint)Params.Value);
            }
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}