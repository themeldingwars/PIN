using System;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB;
using GameServer.Enums;

namespace GameServer.Admin;

[ServerCommand("Add an item to your inventory", "createitem <typeId>", "createitem", "create_item", "giveitem", "give_item")]
public class CreateItemServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (context.SourcePlayer == null && context.SourcePlayer.Inventory == null)
        {
            SourceFeedback("Need a player inventory", context);
            return;
        }

        if (parameters.Length == 0)
        {
            SourceFeedback("No typeId was provided to create item command", context);
            return;
        }
        
        uint typeId = ParseUIntParameter(parameters[0]);
        var itemInfo = SDBInterface.GetRootItem(typeId);
        if (itemInfo == null)
        {
            SourceFeedback("No item data for this typeId", context);
            return;
        }

        uint quantity = 1;
        bool isResource = ((ItemFlags)itemInfo.Flags).HasFlag(ItemFlags.Resource);
        if (isResource)
        {
            if (parameters.Length > 1)
            {
                quantity = Math.Max(1, ParseUIntParameter(parameters[1]));
            }
           
            context.SourcePlayer.Inventory.AddResource(typeId, quantity);
        }
        else
        {
            context.SourcePlayer.Inventory.CreateItem(typeId);
        }

        var msg = new SimulateLootPickup()
        {
            Item = new()
            {
                SdbId = typeId,
                Quantity = (ushort)quantity,
            },
            RewardType = 1,
        };
        context.SourcePlayer.NetChannels[ChannelType.ReliableGss]
                   .SendMessage(msg, context.SourcePlayer.CharacterEntity.EntityId);
    }
}
