using System;
using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class UnpackItemCommand : Command, ICommand
{
    private UnpackItemCommandDef Params;

    public UnpackItemCommand(UnpackItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.PackageSdbId == 0 || Params.ItemSdbId == 0)
        {
            return true;
        }

        if (context.Self is not CharacterEntity { IsPlayerControlled: true } character)
        {
            Console.WriteLine($"Self{context.Self} is not a CharacterEntity");
            return false;
        }

        // todo: consume package by sdb_id and give item by sdb_id to self
        character.Player.Inventory.CreateItem(Params.ItemSdbId);

        return true;
    }
}