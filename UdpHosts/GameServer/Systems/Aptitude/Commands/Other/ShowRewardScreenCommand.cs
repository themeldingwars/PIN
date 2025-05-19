using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class ShowRewardScreenCommand : Command, ICommand
{
    private ShowRewardScreenCommandDef Params;

    public ShowRewardScreenCommand(ShowRewardScreenCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var message = new DisplayRewards() { };

        foreach (var target in context.Targets)
        {
            if (target is not CharacterEntity { IsPlayerControlled: true } character)
            {
                continue;
            }

            character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }

        return true;
    }
}