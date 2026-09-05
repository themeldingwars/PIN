using System.Numerics;
using GameServer.Entities.Character;
using GameServer.StaticDB;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.TinyObject;

public class TinyObjectCreateCommand : Command, ICommand
{
    private TinyObjectCreateCommandDef Params;

    public TinyObjectCreateCommand(TinyObjectCreateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.Type == 0)
        {
            Logger.Warning("Don't know which TinyObject to spawn in {Command} {CommandId}.", nameof(TinyObjectCreateCommand), Params.Id);
            result.SetPass();
            return;
        }

        if (SDBInterface.GetTinyObject(Params.Type) == null)
        {
            Logger.Warning("Invalid TinyObject type {} in {Command} {CommandId}.", Params.Type, nameof(TinyObjectCreateCommand), Params.Id);
            result.SetPass();
            return;
        }

        if (context.Self is CharacterEntity characterEntity)
        {
            var target = context.Self;
            var position = target.Position;
            var orientation = Quaternion.Identity;

            var tinyObjectEntity = context.Shard.EntityMan.SpawnTinyObject(Params.Type, position, characterEntity);
        }
        else
        {
            Logger.Warning("{Command} {CommandId} was called but self is not a character, what do?",  nameof(TinyObjectCreateCommand), Params.Id);
            result.SetPass();
            return;
        }
    }

    public override void Reset(Context context)
    {
        return;
    }
}