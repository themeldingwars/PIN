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
flight profile to glide with. Nothing in the server "launches" the player; the launch is the
client's own prediction of the pad, reported back as a jump pose update.

The effects involved (from `StaticDB/CustomData`):

- **Effect 3418** — `ModifyPermission` 1508827 (`glider_hud: true`) and 1508828 (`glider: true`).
- **Effect 3417** — `SetGliderParameters` 1511094, whose row carries `value: 0`: the pad flies
  the character with profile 0 instead of their own glider. The command now hands the previous
  profile back when the effect ends.
- **Ability 35181** "Glider System - Shared Glider Pad Launch Ability" (plus 36403, 37285,
  38514 for the other pad flavours) is what the client triggers through
  `LocalProximityAbilitySuccess`; its chain acquires everyone in a radius of the pad, applies
  the launch effects, and finishes with four `ImpactRemoveEffect` rows whose definitions carry
  no effect id.

### Known gaps that are not fixed

- `RegisterMovementEffect` (command type 304) is still a placeholder in `Factory`, so a status
  effect cannot bind "while the character is in movement state X keep effect Y". Deployables and
  players still get their effects from the ability chains directly.
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
  away with the animation. If it does not, the difference is in the effect the scope's `Statusfx`
  id points at: `applyeffect <id>` applies it by hand and `listeffects` shows what a character
  carries, which separates "the server never applied it" from "the effect itself does nothing".

## Covered by the test suite

The parts that do not need a client are pinned in `UdpHosts/GameServer.Tests` (they run in CI
with the rest of the suite):

- `ChannelReliableTests` - sequenced channels acknowledge every packet they receive, including
  retransmissions and the ones after a sequence-number wrap; a split message gets each fragment
  acked, is dispatched exactly once, survives a repeated fragment and cannot wedge a channel.
- `CharacterRequirementCommandTests` - the requirement commands keep testing the character of the
  activation when the chain belongs to a deployable, and pass (instead of failing) when there is
  no character in the chain at all.
- `ImpactRemoveEffectCommandTests` - a remove-effect command with no effect id removes nothing
  rather than tearing down the effects of the chain that triggered it.
- `PermissionAndGliderProfileCommandTests` - glider permissions and the glider profile are handed
  back when the effect that granted them ends, and one effect cannot switch off what another
  granted.
- `JumpActionedDetectionTests` - the jump detection on the 16 bit counter is modular, so a long
  fall is not reported as a jump.
