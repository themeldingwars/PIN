using AeroMessages.Matrix;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;
using GameServer.Test;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class TeleportInstanceCommand : Command, ICommand
{
    private TeleportInstanceCommandDef Params;

    public TeleportInstanceCommand(TeleportInstanceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.ZoneId == 0)
        {
            result.SetPass();
            return;
        }

        foreach (var target in context.Targets)
        {
            if (target is not CharacterEntity { Player: NetworkPlayer networkPlayer })
            {
                continue;
            }

            networkPlayer.NetChannels[ChannelType.Matrix].SendMessage(new ExitZone());

            networkPlayer.EnterZone(DataUtils.GetZone(Params.ZoneId));
        }

        result.SetPass();
        return;
    }
}