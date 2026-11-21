using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireItemAttributeCommand : Command, ICommand
{
    private RequireItemAttributeCommandDef Params;

    public RequireItemAttributeCommand(RequireItemAttributeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.AttributeId == 0 || context.Self is not CharacterEntity character)
        {
            result.SetFail(StatusCode.None);
            return;
        }

        if (character.GetItemStat((ushort)Params.AttributeId) != 0f)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail(StatusCode.None);
        }
    }

    public override void Reset(Context context)
    {
    }
}
