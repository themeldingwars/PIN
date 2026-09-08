using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

/// <summary>
///     <c>aptfs::RequirementServerCommandDef</c> (type 110): a gate that names the machines whose local simulation
///     may keep executing the chain — <c>Local</c> the machine simulating the entity the chain runs on,
///     <c>LocalInit</c> the machine simulating the initiator, <c>Server</c> the zone server and <c>Client</c> any
///     client. The flags are for the clients, which run the same chains against their own copy of the static
///     database and use this command to decide whether the feedback tail of a chain (the client-only audio,
///     animation controller and particle commands that follow it) is theirs to run: your own client plays the
///     scope-in sound of your weapon, other clients do not.
///
///     The server is the authority of every chain it executes: it is the zone server, and it is also the only
///     machine simulating the entities involved, so it qualifies as the local machine of the subject and of the
///     initiator as well — the client commands behind the gate are no-ops here either way. Answering the
///     requirement with a failure tore the effect down in the same breath it was applied with: the apply chain
///     of a weapon scope's status effect (<c>dbitems::WeaponScope.Statusfx</c>, e.g. 1313) is
///     StatModifier (run/jump/thrust penalties) → CombatFlags (no sprint) → client anim/audio commands around a
///     <c>RequirementServer Local=1</c> row, so scoping in replicated the effect — the client started the
///     aiming animation and the zoom — and the failed requirement cleared it again an instant later, which the
///     client played out as the weapon blending back to hip fire while the zoom and the fire mode stayed.
/// </summary>
public class RequirementServerCommand : Command, ICommand
{
    private RequirementServerCommandDef Params;

    public RequirementServerCommand(RequirementServerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}
