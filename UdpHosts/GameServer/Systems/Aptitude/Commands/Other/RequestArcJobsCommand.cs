using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RequestArcJobsCommand : Command, ICommand
{
    private RequestArcJobsCommandDef Params;

    public RequestArcJobsCommand(RequestArcJobsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // var message = ?
        foreach (var target in context.Targets)
        {
            if (target is not CharacterEntity character)
            {
                continue;
            }

            // character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }

        return true;
    }
}