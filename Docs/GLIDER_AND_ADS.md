# Gliding and aiming down sights: what the server owns

Two client abilities that look entirely client-side (a glider deploy, aiming down the
sights of a weapon) both hang off server replicated state, and both were broken by that
state never being written, or never being taken away again. This document is the map of
the server side of them.

## Gliding

| State | Where it lives | Written by |
|---|---|---|
| Flight profile (turn rate, thrust, stall speeds) | `Character_CombatController.GliderProfileIdProp`, a row of `dbcharacter::GliderParameters` | `SetGliderParameters` (command type 247), through `CharacterEntity.SetGliderProfileId` |
| Permission to deploy wings at all | `PermissionFlags` bit `glider` (and `glider_hud` for the HUD) on the combat controller | `ModifyPermission` (the glider permission rows are 1508827/1508828, granted by status effect 3418) |
| Which movement state the character is in | `MovementStateProp`, decoded by `MovementStateContainer` (`Movestate.Glider` is `0x70`) | the client, through its `MovementInput` pose updates — the server only mirrors |
| Whether a landing kills you | `FallDamageSystem`, which exempts a landing when the character was in `Glider` or `Jetpack` | the client pose, same field as above |

So a player who is standing on a boost panel needs two things from the server, and both are
applied as **status effects** rather than as movement code: the permission to glide, and the
flight profile to glide with. Nothing in the server "launches" the player across a *static*
panel (that launch is the client's own prediction of the panel, reported back as a jump pose
update). A *deployable* pad, though, runs its launch ability server-side, and its launch
effect chain ends in a `ForcePush` (effect 8097, row 1509142) that tells the client to push
the character straight up with the row's strength; the push also has to be strong enough to
carry the character out of the pad's trigger radius, or the client keeps re-triggering the
pad and re-playing the launch effects while the character stands there.

Two parts of that chain are easy to get wrong on a server that only has part of the data:

- The launch ability's chain loads a register value from a named variable (`WingFX`, row
  1001663, `LoadRegisterFromNamedVar`). The server has no named-variable store, and the
  command used to be a no-op that left the register unset, so the register comparisons the
  launch effects use to select their effect level never saw the value the data expects (they
  fall back to their last row when no branch matches, which made an unset register and a
  real level indistinguishable). The command now loads the row's `undecl_value` fallback
  (1.0 for the pad) through the row's `regop`, like `SetRegister` does.
- `ForcePush` used to ignore its row entirely and hard-code a vertical +45; then it sent the
  row's strength but on a `ForcedMovement` whose `[Time1, Time2]` window was 1 ms long — the
  window expires before or while the packet is in flight, so the client discarded the impulse
  and every pad "played the animation but gave no launch". The window is now real: it starts
  50 ms out (so the push survives latency) and holds 500 ms, the same 500 ms the launch
  effect's own `restrict_movement` runs for. `strength_regop` (add) folds the chain register
  into the strength, which is how the pad module rows (+3 per Lofty module, +10 for the
  boosted variants) reach the push; the register is non-negative there, so the fold can only
  strengthen the base 30. `RequireHasItem` is implemented for the module checks — it used to
  be a placeholder that passed for everyone, which would have handed all four modules' bonus
  to every launch.
- The launch effect's apply chain sets `restrict_movement` through `CombatFlags` (row 1509150)
  for the 500 ms the launch runs. `CombatFlags` (command type 64) used to be a stub in
  `Factory`, so the flag was never replicated: the client never locked the character's own
  ground movement, which let the player's input fight (and cancel) the forced launch impulse.
  The command now writes the flags its row carries and restores only those bits when the
  effect ends, the same snapshot-and-restore the other actives use.

The effects involved (from `StaticDB/CustomData`):

- **Effect 3418** — `ModifyPermission` 1508827 (`glider_hud: true`) and 1508828 (`glider: true`).
- **Effect 3417** — `SetGliderParameters` 1511094, whose row carries `value: 18` — the same
  `dbcharacter::GliderParameters` profile (18) the normal glider effect grants. The client table has
  no profile 0 (its ids run from 4 up), so a pad effect that handed the client profile 0 handed it a
  flight model that does not exist: wings could deploy but nothing could fly. The command now hands
  the previous profile back when the effect ends, and rows that carry no value (or 0) leave the
  profile alone.
- **Ability 35181** "Glider System - Shared Glider Pad Launch Ability" (plus 36403, 37285,
  38514 for the other pad flavours) is what the client triggers through
  `LocalProximityAbilitySuccess`; its chain acquires everyone in a radius of the pad, applies
  the launch effects, and finishes with four `ImpactRemoveEffect` rows whose definitions carry
  no effect id.

- `RegisterMovementEffect` (command type 304) is now implemented. The same chain runs on the
  client and on the server, and the definition's `on_client`/`on_server` flags say which
  machine performs the registration. The pad's rows (1508976/1508977) register the glider
  flight effect (723, audio and particles) with `on_client=1, on_server=0`, so the server
  steps over them — that effect is client-side only. The rows with `on_server=1` (the sprint
  effect 7 for the running state, and a handful of others) bind an effect the ability system
  then keeps applied while the character is in the bound movement state and takes off again
  when it leaves (or the carrying effect ends). The bound state index is the client's
  movestate nibble (`Movestate >> 4`: 1 standing … 7 glider, 8 glider thrusters, …).

### Known gaps that are not fixed

- Most rows of `aptgss::SetGliderParametersCommandDef` carry no value at all; those leave the
  character's profile untouched, which is the safest reading of an incomplete table.
- Grants of the same permission by two different effects are not reference counted: the first
  effect to expire takes the permission away from both.

## Aiming down sights

The client sends `GssCharacterCommand.UseScope` (`Time`, `InScope`) when the player scopes in
and out, and `SelectFireMode` / `SelectWeapon` for the weapon's fire modes. The server owns:

| State | Field | Meaning |
|---|---|---|
| Selected fire mode of the weapon | `Character_CombatController.FireMode_0Prop` | written by `SelectFireMode`; decides main vs. underbarrel weapon (`IsAltFireMode`) |
| Scoped or not | `Character_CombatController.FireMode_1Prop` | written by `UseScope`, normalised to `0`/`1` because `InScope` is an `sbyte` |
| The scope's own status effect | the character's `StatusEffects_N` fields | written by `UseScope` through `CharacterEntity.SetScopedState` |
| Scope bubble (camera state) | `Character_BaseController.ScopeBubbleInfoProp` | written by `SetScopeBubble` (command type 184) when its definition carries a layer |

The row that used to be missing entirely is the **status effect of the scope**: a weapon's
scope attachment (`dbitems::WeaponScope`) has a `Statusfx` column, and that effect is what the
client ties the scoped view to. `SDBUtils.GetDetailedWeaponInfo` already resolved it (into
`WeaponInfoResult.ScopeStatusFx`) and nothing consumed it, so the server never applied it, never
removed it, and the client's predicted zoom had no server side owner that could take it away:
scoping out reverted the animation (the `FireMode_1` field) while the zoom stayed. `UseScope`
now applies the effect while the character is scoped in and removes it on scope out, and a
weapon or fire-mode switch always clears it.

Applying it exposed the second half of the bug, in the effect's own apply chain. The scope
effects (1313/1314/15347, and 102 without this row) are
`StatModifier` (run speed ×0.5, jump height ×0.6, jet thrust ×0.5) → `CombatFlags`
(`restrict_sprint`) → client animation/audio commands, and between the client commands sits a
`RequirementServer` row with `Local=1` and everything else 0. That command names the machines
whose local simulation may keep executing a chain — the flags are for the clients, which run
the same chains against their own copy of the database to decide whether the feedback tail
after the gate (the scope-in sound, the aim pose) is theirs to play. The server used to answer
every row without `Server=1` with a failure, which aborted the apply chain and cleared the
effect again in the same breath it had been replicated with: the client started the scoped view,
then received the removal and blended the weapon back to hip fire while the zoom and
`FireMode_1` stayed — the "ADS applies, then the animation plays back to hip fire" report. The
server is the authority of every chain it executes (it is the zone server *and* the only machine
simulating the entities), so `RequirementServerCommand` passes on the server now.

The `CombatFlags` row right before the gate was the other half of the same report:
`CombatFlags` (command type 64) used to be a stub in `Factory` too, so `restrict_sprint` (row
1605139) — the "no sprint while aiming" flag the client reads off the combat controller — was
never replicated. The client, allowed to sprint, blended the weapon out of the aim pose back to
hip fire while the zoom and `FireMode_1` stayed. The command now writes the flags its row
carries and, on removal, restores only those bits so an effect cannot clear a flag another
still-running effect set.

The effect's duration chain (`BattleFrameDuration` → `RequireCState living` → the client's
`tfRequireServerConfirmed`) keeps it alive for as long as the character is living, so nothing
but scoping out, switching weapon or fire mode, or dying takes the scoped state away. The
last step became its own failure mode: per `apt::CommandType` that requirement runs on the
**client** (`environment: client`), where it exists to guard locally predicted effects — the
client applies the scope effect the moment RMB is pressed and only keeps its prediction
alive while a matching server confirmation is visible. The match key is the effect's start
time, and PIN used to stamp every effect with a fresh server time (`Shard.CurrentTime` at
apply), which can never equal the client's own timestamp from its `UseScope` message — the
confirmation never matched, so the client tore down its local copy and snapped back to hip
fire. The replicated start time now prefers `Context.InitTime` — which for everything the
client initiated carries *its* clock (ability activation has always flowed that way; the
scope path now feeds `UseScope.Time` into it the same way) — with server time as the
fallback for server-initiated applications.

`SetScopeBubble` (which shows up in the same chains, including the glider effects) is a command
whose table we only have ids for, so it writes the character's `ScopeBubbleInfo` only when a row
actually carries a layer value and clears it when the effect ends. Recovering the columns of
`aptgss::SetScopeBubbleCommandDef` is what would let the layer come from the data.

## Checking either one in game

- Boost panel: the launch and the wings deploy play, the client stays connected, and the server
  log does **not** repeat `RequireCStateCommand ... fails because source is not a Character` or
  `Don't know which effect to remove` while standing on the pad. See
  `GLIDER_PAD_CONNECTION_PROBLEM.md` for the connection side.
- Glider after the launch: the `glider` permission and the glider profile are both visible in
  the debug output (`dbg_weapon` prints the resolved weapon, `listeffects` the effects a
  character carries), and by falling from height: a glide that engages must not do fall
  damage (`FallDamageSystem` exempts `Movestate.Glider`).
- ADS: scope in and out repeatedly on a weapon with an underbarrel (rifle). The zoom must go
  away with the animation, and while the player holds the aim the weapon has to *stay* in the
  aim pose — it must not blend back to hip fire on its own after a moment, which is the
  client's `tfRequireServerConfirmed` duration check dropping its predicted scope effect
  because the server confirmation carries a different timestamp (the zone log's
  `[Scope] UseScope ... Time=<client> ... ServerTime=<server>` line shows both clocks next to
  each other). Holding aim also carries the
  effect's own penalties: sprint is disallowed and run speed is halved (`listeffects` shows the
  effect a character carries, `applyeffect <id>` applies it by hand, which separates "the server
  never applied it" from "the effect itself does nothing").

## Covered by the test suite

The parts that do not need a client are pinned in `UdpHosts/GameServer.Tests` (they run in CI
with the rest of the suite):

- `ChannelReliableTests` — sequenced channels acknowledge every packet they receive, including
  retransmissions and the ones after a sequence-number wrap; a split message gets each fragment
  acked, is dispatched exactly once, survives a repeated fragment and cannot wedge a channel.
- `CharacterRequirementCommandTests` — the requirement commands keep testing the character of the
  activation when the chain belongs to a deployable, and pass (instead of failing) when there is
  no character in the chain at all.
- `ImpactRemoveEffectCommandTests` — a remove-effect command with no effect id removes nothing
  rather than tearing down the effects of the chain that triggered it.
- `PermissionAndGliderProfileCommandTests` — glider permissions and the glider profile are handed
  back when the effect that granted them ends, and one effect cannot switch off what another
  granted.
- `RequirementServerCommandTests` — the `RequirementServer` gate passes on the server for every
  machine flag row the table carries, the scope effect's apply chain (stat penalties, no-sprint,
  client feedback) succeeds as a whole and registers its modifiers, and its duration chain keeps
  the effect alive while the character is living.
- `CombatFlagsCommandTests` — `CombatFlags` writes the flags its row carries, restores only those
  bits (not flags another effect set meanwhile) when it ends, and steps over a deployable owner.
- `RegisterMovementEffectCommandTests` — `RegisterMovementEffect` steps over client-side rows (the
  pad's glider-effect registrations), registers server-side rows only for a character, and reads
  the movestate nibble (and the sprint flag) the bound state is matched on.
- `JumpActionedDetectionTests` — the jump detection on the 16 bit counter is modular, so a long
  fall is not reported as a jump.
