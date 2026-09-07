using System.Collections.Generic;
using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

/// <summary>
/// CommandType ("Modify Permission"), the command that grants and takes away the client side permissions of a
/// character (gliding, the glider hud, the jetpack, ...).
///
/// The flags are plain state on the character, so a removal has to put back what the effect replaced. Writing
/// the negation of the granted value (the behaviour until here) does that by accident only for effects that
/// grant a permission: any other effect that had set the same flag first is silently reset to "false" when this
/// effect ends, which is how a boost pad used to take the glider away from the player's own glider effect.
/// The value from before the effect applied is remembered in the active context instead and restored on removal.
///
/// Note that grants are still not reference counted: if two effects grant the *same* flag, the first one to
/// expire already takes it away from the second one.
/// </summary>
public class ModifyPermissionCommand : Command, ICommand
{
    private ModifyPermissionCommandDef Params;

    public ModifyPermissionCommand(ModifyPermissionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self; // NOTE: Based on glider, it seems like it should use self, maybe that is reasonable for all 'active' style commands?
        if (target is CharacterEntity character)
        {
            context.Actives.Add(this, new ModifyPermissionActiveContext(ReadPermissions(character)));
        }
        else
        {
            Logger.Debug("[{Command} {CommandId}] does nothing because self is {SelfType}",
                nameof(ModifyPermissionCommand), Params.Id, target?.GetType().Name ?? "nothing");
        }

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        if (context.Self is not CharacterEntity character)
        {
            return;
        }

        // The apply chain may have run other commands first, so the snapshot is taken here, right before the
        // values are overwritten. Anything captured earlier is only a fallback.
        if (activeCommandContext is ModifyPermissionActiveContext active)
        {
            active.Previous = ReadPermissions(character);
        }

        if (Params.Glider != null)
        {
            character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.glider, (bool)Params.Glider);
        }

        if (Params.GliderHud != null)
        {
            character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.glider_hud, (bool)Params.GliderHud);
        }

        /*if (Params.Hover != null)
        {
            character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.unk, (bool)Params.Hover);
        }*/

        if (Params.Jetpack != null)
        {
            character.SetPermissionFlag(PermissionFlagsData.CharacterPermissionFlags.jetpack, (bool)Params.Jetpack);
        }
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        if (context.Self is not CharacterEntity character)
        {
            return;
        }

        var previous = (activeCommandContext as ModifyPermissionActiveContext)?.Previous;

        Restore(character, PermissionFlagsData.CharacterPermissionFlags.glider, previous);
        Restore(character, PermissionFlagsData.CharacterPermissionFlags.glider_hud, previous);
        Restore(character, PermissionFlagsData.CharacterPermissionFlags.jetpack, previous);
    }

    private static Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool> ReadPermissions(CharacterEntity character)
    {
        return new Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool>
        {
            { PermissionFlagsData.CharacterPermissionFlags.glider, character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider] },
            { PermissionFlagsData.CharacterPermissionFlags.glider_hud, character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.glider_hud] },
            { PermissionFlagsData.CharacterPermissionFlags.jetpack, character.CurrentPermissions[PermissionFlagsData.CharacterPermissionFlags.jetpack] },
        };
    }

    private void Restore(CharacterEntity character, PermissionFlagsData.CharacterPermissionFlags flag, Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool> previous)
    {
        // Only the flags this command actually touched are restored.
        bool touched = flag switch
        {
            PermissionFlagsData.CharacterPermissionFlags.glider => Params.Glider != null,
            PermissionFlagsData.CharacterPermissionFlags.glider_hud => Params.GliderHud != null,
            PermissionFlagsData.CharacterPermissionFlags.jetpack => Params.Jetpack != null,
            _ => false,
        };

        if (!touched)
        {
            return;
        }

        if (previous != null && previous.TryGetValue(flag, out bool value))
        {
            character.SetPermissionFlag(flag, value);
        }
        else
        {
            // No snapshot to fall back on (an effect applied before this command was loaded with one): the old
            // behaviour of writing the opposite of the granted value is still better than leaving a permission
            // granted forever.
            character.SetPermissionFlag(flag, !GrantedValue(flag));
        }
    }

    private bool GrantedValue(PermissionFlagsData.CharacterPermissionFlags flag)
    {
        return flag switch
        {
            PermissionFlagsData.CharacterPermissionFlags.glider => Params.Glider == true,
            PermissionFlagsData.CharacterPermissionFlags.glider_hud => Params.GliderHud == true,
            PermissionFlagsData.CharacterPermissionFlags.jetpack => Params.Jetpack == true,
            _ => false,
        };
    }

    /// <summary>
    /// The permissions of the character before this effect changed them.
    /// </summary>
    private class ModifyPermissionActiveContext : ICommandActiveContext
    {
        public ModifyPermissionActiveContext(Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool> previous)
        {
            Previous = previous;
        }

        public Dictionary<PermissionFlagsData.CharacterPermissionFlags, bool> Previous { get; set; }
    }
}