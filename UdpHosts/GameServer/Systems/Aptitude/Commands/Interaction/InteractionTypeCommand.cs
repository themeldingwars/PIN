using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class InteractionTypeCommand : ICommand
{
    private InteractionTypeCommandDef Params;

    public InteractionTypeCommand(InteractionTypeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Targets.Count > 0)
        {
            var interactionEntity = context.Targets.First();
            var hack = interactionEntity as BaseEntity;
            var type = hack.Interaction.Type;

            return (byte)type == Params.Type;
        }
        else
        {
            return false;
        }
    }
}