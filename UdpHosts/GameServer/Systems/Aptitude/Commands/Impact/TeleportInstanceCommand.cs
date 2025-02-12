using AeroMessages.Matrix.V25;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;
using GameServer.Test;

namespace GameServer.Aptitude;

public class TeleportInstanceCommand : Command, ICommand
{
    private TeleportInstanceCommandDef Params;

    public TeleportInstanceCommand(TeleportInstanceCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.ZoneId == 0)
        {
            return true;
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

        return true;
    }
}