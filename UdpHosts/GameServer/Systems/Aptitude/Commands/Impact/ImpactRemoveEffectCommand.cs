using System;
using System.Linq;
using System.Text;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ImpactRemoveEffectCommand : Command, ICommand
{
    private ImpactRemoveEffectCommandDef Params;

    public ImpactRemoveEffectCommand(ImpactRemoveEffectCommandDef par)
: base(par)
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
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("Active Effects (Self):");
            var character = context.Self;
            var activefx = character.GetActiveEffects();
            foreach (var activeEffect in activefx)
            {
                if (activeEffect != null)
                {
                    stringBuilder.AppendLine($"{activeEffect.Index} : {activeEffect.Effect?.Id}");
                }
            }

            string message = stringBuilder.ToString();
            Console.WriteLine(message);
            Console.WriteLine($"Don't know which effect to remove for ImpactRemoveEffectCommand {Params.Id}");
        }

        return true;
    }
}