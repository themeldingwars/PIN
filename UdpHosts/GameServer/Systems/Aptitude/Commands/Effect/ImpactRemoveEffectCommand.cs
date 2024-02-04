using System;
using System.Linq;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ImpactRemoveEffectCommand : ICommand
{
    private ImpactRemoveEffectCommandDef Params;

    public ImpactRemoveEffectCommand(ImpactRemoveEffectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (Params.EffectId != null)
        {
            uint effectId = (uint)Params.EffectId;
            if (Params.RemoveFromSelf != null && Params.RemoveFromSelf == true)
            {
                context.Abilities.DoRemoveEffect(context.Self, effectId);
            }
            else
            {
                foreach (IAptitudeTarget target in context.Targets)
                {
                   context.Abilities.DoRemoveEffect(target, effectId);
                }
            }
        }
        else
        {
            Console.WriteLine($"Don't know which effect to remove for ImpactRemoveEffectCommand {Params.Id}");
        }

        return true;
    }
}