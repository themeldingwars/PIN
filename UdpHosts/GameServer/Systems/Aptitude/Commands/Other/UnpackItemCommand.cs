using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class UnpackItemCommand : Command, ICommand
{
    private UnpackItemCommandDef Params;

    public UnpackItemCommand(UnpackItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.PackageSdbId == 0 || Params.ItemSdbId == 0)
        {
            result.SetPass();
            return;
        }

        if (context.Self is not CharacterEntity { IsPlayerControlled: true } character)
        {
            Logger.Warning("{Command} {CommandId} Self {Self} is not a CharacterEntity", nameof(UnpackItemCommand), Params.Id, context.Self);
            result.SetFail();
            return;
        }

        // todo: consume package by sdb_id and give item by sdb_id to self
        character.Player.Inventory.CreateItem(Params.ItemSdbId);

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}