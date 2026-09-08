using AeroMessages.GSS.Character;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.SetFlags;

/// <summary>
/// <c>aptfs::CombatFlagsCommandDef</c> (command type 64): writes the combat flags the client reads from the
/// character's combat controller for the lifetime of the effect that carries this command. Two of the effects
/// behind the reported bugs hang off it:
///
/// - The glider pad's launch effect (8097) sets <c>restrict_movement</c> while the 500 ms launch runs, so the
///   forced launch velocity is not fought by the character's own ground movement.
/// - A weapon scope's status effect (e.g. 1313) sets <c>restrict_sprint</c> for as long as the player aims.
///
/// The flags are plain replicated state on the character, so a removal has to put back exactly what the effect
/// replaced. Writing a hard-coded zero would silently clear any flag another still-running effect had set, so
/// the value from before the effect applied is remembered in the active context and only the bits this row owns
/// are restored on removal — every other flag keeps whatever the still-running effects set. This is the same
/// snapshot-and-restore pattern <c>ModifyPermission</c> uses for the same reason, with the same caveat: grants
/// are not reference counted, so two effects that set the *same* flag make the first one to expire take it away
/// from both.
/// </summary>
public class CombatFlagsCommand : Command, ICommand
{
    private CombatFlagsCommandDef Params;

    private CombatFlagsData.CharacterCombatFlags _setFlags;

    public CombatFlagsCommand(CombatFlagsCommandDef par)
: base(par)
    {
        Params = par;
        _setFlags = ResolveFlags(par);
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (target is CharacterEntity)
        {
            context.Actives.Add(this, new CombatFlagsActiveContext());
        }
        else
        {
            // Deployable owned chains (a pad running its launch ability) reach this command with the pad as
            // self. The flags belong on the character that is being launched, and the chain has already made
            // that character the target; step over it the same way the other actives do instead of failing.
            Logger.Debug("[{Command} {CommandId}] does nothing because self is {SelfType}",
                nameof(CombatFlagsCommand), Params.Id, target?.GetType().Name ?? "nothing");
        }

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        if (context.Self is not CharacterEntity character)
        {
            return;
        }

        if (activeCommandContext is not CombatFlagsActiveContext active)
        {
            return;
        }

        // The apply chain may have run other commands first, so the snapshot is taken here, right before the
        // flags are written, the same way ModifyPermission does it.
        active.Previous = character.CombatFlags.Value;

        character.SetCombatFlags(new CombatFlagsData
        {
            Value = active.Previous | _setFlags,
            Time = context.Shard.CurrentTime,
        });
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        if (context.Self is not CharacterEntity character)
        {
            return;
        }

        if (activeCommandContext is not CombatFlagsActiveContext active)
        {
            return;
        }

        // Only the bits this row owns are restored: clearing the whole value would silently take away flags
        // other still-running effects set while this one was applied. The current value keeps every other bit,
        // and the flags this command set fall back to what they were before the effect applied.
        var current = character.CombatFlags.Value;
        var restored = (current & ~_setFlags) | (active.Previous & _setFlags);

        character.SetCombatFlags(new CombatFlagsData
        {
            Value = restored,
            Time = context.Shard.CurrentTime,
        });
    }

    /// <summary>
    ///     Maps the definition's flag columns onto the replicated <see cref="CombatFlagsData.CharacterCombatFlags" />
    ///     bits. Two columns (<c>immune_physics</c>, <c>immune_death</c>) have no known client bit and are left
    ///     alone rather than guessed at.
    /// </summary>
    private static CombatFlagsData.CharacterCombatFlags ResolveFlags(CombatFlagsCommandDef par)
    {
        CombatFlagsData.CharacterCombatFlags flags = 0;

        if (par.ReversedControls == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.reversed_controls;
        }

        if (par.ImmuneFalldamage == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.immune_falldamage;
        }

        if (par.RestrictMovement == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.restrict_movement;
        }

        if (par.RestrictWeapon == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.restrict_weapon;
        }

        if (par.RestrictAbilities == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.restrict_abilities;
        }

        if (par.KnockDown == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.knock_down;
        }

        if (par.MoveThroughObjects == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.move_through_objects;
        }

        if (par.RestrictSprint == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.restrict_sprint;
        }

        if (par.RestrictMelee == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.restrict_melee;
        }

        if (par.RestrictInteraction == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.restrict_interaction;
        }

        if (par.RemoveHitboxes == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.remove_hitboxes;
        }

        if (par.RestrictStumble == 1)
        {
            flags |= CombatFlagsData.CharacterCombatFlags.restrict_stumble;
        }

        return flags;
    }

    private class CombatFlagsActiveContext : ICommandActiveContext
    {
        public CombatFlagsData.CharacterCombatFlags Previous;
    }
}
