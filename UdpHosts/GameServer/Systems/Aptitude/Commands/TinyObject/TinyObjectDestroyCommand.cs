using GameServer.Entities.TinyObject;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.TinyObject;

public class TinyObjectDestroyCommand : Command, ICommand
{
    private TinyObjectDestroyCommandDef Params;

    public TinyObjectDestroyCommand(TinyObjectDestroyCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is TinyObjectEntity tinyObject)
        {
            var shard = context.Shard;
            shard.Post(() =>
            {
                if (shard.Entities.ContainsKey(tinyObject.EntityId))
                {
                    shard.EntityMan.Remove(tinyObject.EntityId);
                }
            });
        }
        else
        {
            Logger.Warning("{Command} {CommandId} was called with self {Self}, which is not a tiny object", nameof(TinyObjectDestroyCommand), Params.Id, context.Self);
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
        return;
    }
}
