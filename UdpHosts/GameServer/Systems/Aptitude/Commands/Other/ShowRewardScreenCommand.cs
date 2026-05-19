using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ShowRewardScreenCommand : Command, ICommand
{
    private ShowRewardScreenCommandDef Params;

    public ShowRewardScreenCommand(ShowRewardScreenCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
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

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}