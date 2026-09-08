using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Movement;

/// <summary>
/// <c>aptfs::RegisterMovementEffectCommandDef</c> (command type 304): binds a status effect to a movement state
/// for the lifetime of the effect that carries this command, so the bound effect is applied while the character
/// is in that state and removed when it leaves it (or the carrying effect ends).
///
/// The command runs on both machines, and the definition's <c>OnClient</c>/<c>OnServer</c> flags say which side
/// actually performs the registration:
///
/// - The glider pad's launch chains register the glider flight effect (723) for the <c>Glider</c> (index 7) and
///   <c>GliderThrusters</c> (index 8) states with <c>on_client=1</c>, <c>on_server=0</c>: the flight effect is
///   client-side only (its chains are audio and particles with a <c>RequireMovestate</c> duration), so the
///   server must NOT apply it — stepping over such a row with a <c>true</c> is the correct server behaviour, not
///   a gap.
/// - The rows with <c>on_server=1</c> (e.g. the sprint effect 7 for the running state) bind effects the server
///   has to apply itself.
///
/// The movement state index is the client's movestate nibble: <c>(int)Movestate &gt;&gt; 4</c>, i.e. 1 standing,
/// 2 running, 3 falling, 4 sliding, 5 walking, 6 jetpack, 7 glider, 8 glider thrusters, 9 glider stall,
/// 10 knockdown, 11 knockdown falling, 12 jetpack sprint.
/// </summary>
public class RegisterMovementEffectCommand : Command, ICommand
{
    private RegisterMovementEffectCommandDef Params;

    public RegisterMovementEffectCommand(RegisterMovementEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Self is not CharacterEntity)
        {
            Logger.Debug("[{Command} {CommandId}] does nothing because self is {SelfType}",
                nameof(RegisterMovementEffectCommand), Params.Id, context.Self?.GetType().Name ?? "nothing");

            return true;
        }

        if (Params.OnServer != 1)
        {
            // The client runs its own copy of this chain and performs the registration against its local
            // simulation; the server only registers server-side effects (OnServer == 1). The glider pad's rows
            // are client-side (effect 723 is audio and particles), so the server steps over them here.
            Logger.Debug("[{Command} {CommandId}] is client-side (statusfx {StatusfxId}, movestate {MovestateIndex}), server skips it",
                nameof(RegisterMovementEffectCommand), Params.Id, Params.StatusfxId, Params.MovestateIndex);

            return true;
        }

        context.Actives.Add(this, new RegisterMovementEffectActiveContext());

        return true;
    }

    public void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
        if (context.Self is not CharacterEntity character || Params.OnServer != 1)
        {
            return;
        }

        if (activeCommandContext is not RegisterMovementEffectActiveContext active)
        {
            return;
        }

        active.Registration = context.Abilities.RegisterMovementEffect(character, Params.StatusfxId, (byte)Params.MovestateIndex, Params.Sprinting == 1);

        // The ability system re-evaluates registrations on its tick, but apply right away when the character is
        // already in the bound state so a launch effect that fires mid-state does not wait a tick for its visual.
        if (IsInState(character))
        {
            context.Abilities.DoApplyEffect(Params.StatusfxId, character, context);
        }
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        if (activeCommandContext is not RegisterMovementEffectActiveContext { Registration: not null } active)
        {
            return;
        }

        context.Abilities.UnregisterMovementEffect(active.Registration);
        active.Registration = null;

        if (context.Self is CharacterEntity character)
        {
            context.Abilities.DoRemoveEffect(character, Params.StatusfxId);
        }
    }

    internal bool IsInState(CharacterEntity character)
    {
        var index = (byte)((int)character.MovementStateContainer.Movestate >> 4);

        return index == Params.MovestateIndex
            && (Params.Sprinting != 1 || character.MovementStateContainer.Sprint);
    }

    private class RegisterMovementEffectActiveContext : ICommandActiveContext
    {
        public MovementEffectRegistration Registration;
    }
}
