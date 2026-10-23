using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.NPC;

public class NPCSpawnCommand : Command, ICommand
{
    private NPCSpawnCommandDef Params;

    public NPCSpawnCommand(NPCSpawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.MonsterId == 0)
        {
            result.SetPass();
            return;
        }

        var owner = Params.SetOwner ? context.Self as CharacterEntity : null;
        var level = Params.Level != 0 ? Params.Level : CharacterEntity.DefaultLevel;

        var spawned = context.Shard.EntityMan.SpawnCharacter(Params.MonsterId, context.InitPosition, owner, level: level, hidden: Params.Hidden);
        if (spawned == null)
        {
            Logger.Warning("{Command} {CommandId} failed to spawn monster {MonsterId}", nameof(NPCSpawnCommand), Params.Id, Params.MonsterId);
            result.SetFail();
            return;
        }

        context.Targets.Push(spawned);
        result.SetPass();
        return;
    }
}
