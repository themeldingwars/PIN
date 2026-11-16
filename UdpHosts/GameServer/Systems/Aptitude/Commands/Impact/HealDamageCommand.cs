using AeroMessages.GSS.Character;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class HealDamageCommand : Command, ICommand
{
    private HealDamageCommandDef Params;

    public HealDamageCommand(HealDamageCommandDef par)
: base(par)
    {
        Params = par;
    }

    // Assumption: Weapondamage reads Healpoints as a percentage of the weapon's damage, the same as InflictDamage does.
    // Assumption: Usedmgdealt and DamageType don't change the amount. The Health Powerup sets Usedmgdealt and heals the
    // 20 % the replays show without it.
    public override void Execute(Context context, ref CommandResult result)
    {
        var amount = (int)AbilitySystem.RegistryOp(context.Register, Params.Healpoints, (Operand)Params.HealpointsRegop);
        if (Params.Weapondamage == 1)
        {
            amount = (context.DamagePoints * amount) / 100;
        }

        if (amount > 0)
        {
            foreach (IAptitudeTarget target in context.Targets)
            {
                // Assumption: a heal doesn't reach the dead
                if (target is CharacterEntity { CharacterState.State: CharacterStateData.CharacterStatus.Dead } || target is not IEntity entity)
                {
                    continue;
                }

                context.Shard.Damage.ApplyHeal(entity, amount, context.Initiator as IEntity);
            }
        }

        result.SetPass();
    }

    public override void Reset(Context context)
    {
    }
}
