using GameServer.StaticDB;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Create a tiny object", "tiny <typeId>", "tiny")]
public class DebugTinyObjectServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 1)
        {
            SourceFeedback("Invalid number of parameters for debug tiny object command", context);
            return;
        }

        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot create tiny object without a valid player character", context);
            return;
        }

        uint typeId = ParseUIntParameter(parameters[0]);
        if (SDBInterface.GetTinyObject(typeId) == null)
        {
            SourceFeedback("No tiny object with this id", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;
        var pos = context.SourcePlayer.CharacterEntity.Position;
        var shard = context.Shard;
        shard.EntityMan.SpawnTinyObject(typeId, pos, character);
    }
}