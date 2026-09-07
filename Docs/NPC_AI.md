# NPC AI Dev Notes

This document explains the server side NPC AI: what a spawned mob actually does,
where the code lives, how to tune it and how to exercise it from the game client.

It replaces the old `AIEngine` stub, which was an empty `Tick`. Spawned mobs now
pick a target, walk towards it, shoot at it and give up when they are dragged too
far from where they spawned.

> Character origins sit at the feet (the muzzle offset is `(0.2, 0, 1.62)` in
> `CalculateProjectileOrigin`, i.e. chest height above the origin) and the body
> orientation is a yaw-only rotation about world +Z, confirmed against the
> spawn points in `StaticDB/CustomData/outpost.json`. Ground snapping is **on by
> default** - see [Tuning](#5-tuning).

---

## 1. Where the code lives

```
UdpHosts/GameServer/Systems/Ai/
├── AiEngine.cs                 shard integration: targets, movement, attacks, events
├── AiBrain.cs                  the state machine (no shard/entity/physics dependency)
├── AiBrainState.cs             Idle / Chase / Attack / Return / Dead
├── AiPerception.cs             what the engine tells a brain about the world
├── AiDecision.cs               what a brain tells the engine to do
├── IAiRules.cs                 tunables
├── StandardAiRules.cs          the shipped defaults
├── AiSpeeds.cs                 monster row speed -> metres per second
├── AiVectors.cs                horizontal distance + character facing maths
├── IAiHostility.cs             who counts as an enemy
├── FactionAiHostility.cs       SDB faction table implementation
├── IAiMonsterStats.cs          where movement speeds come from
├── SdbAiMonsterStats.cs        reads dbcharacter::Monster
├── IAiAttackFeedback.cs        cosmetic hit messages
└── HitFeedbackAttackFeedback.cs routes them through CombatSim.HitFeedback
```

`Shard` owns the engine as `Shard.AI` and ticks it first in `Shard.Tick`, before
`Physics` and `EntityManager`. Every character that goes through
`EntityManager.SpawnCharacter` is registered automatically, and
`EntityManager.Remove` unregisters it. Player controlled characters are refused by
`AiEngine.Register`.

The split matters for testing: `AiBrain` is pure decision logic fed an
`AiPerception` snapshot, so the whole state machine is covered by unit tests in
`GameServer.Tests/AiBrainTests.cs` with no shard, entity or physics involved.
`AiEngineTests.cs` drives the real engine against the in-memory `FakeShard`.

---

## 2. The state machine

```
                target inside aggro radius
        ┌─────────────────────────────────────────────┐
        │                                             ▼
     ┌──────┐  dragged past leash  ┌────────┐     ┌───────┐
     │ Idle │ ───────────────────► │ Return │     │ Chase │
     └──────┘                      └────────┘     └───────┘
        ▲                             │  arrived      │  ▲
        │  target lost / died         │  home         │  │ out of range
        └─────────────────────────────┴───────────────┘  │ or no line of sight
                                            ▼            │
                                        ┌────────┐       │
                                        │ Attack │ ──────┘
                                        └────────┘
```

* **Idle** - no target. The NPC stands where it was spawned.
* **Chase** - a hostile player was acquired; the NPC walks towards them until it is
  inside `StandoffRange`.
* **Attack** - the target is inside `AttackRange` **and** visible. The NPC faces
  the target, closes to `StandoffRange` and fires every `AttackCooldownMs`.
* **Return** - the NPC was pulled more than `LeashRadius` from its spawn point. It
  drops its target and walks home, then goes back to Idle.
* **Dead** - terminal. Set from the `CharacterDiedEvent`.

Two details worth knowing:

* **Entry and exit ranges differ.** A target is acquired at `AttackRange` but only
  released at `AttackRangeExit` (45 m in, 52 m out). Without that hysteresis a
  player walking along the edge of the range flips the state every tick.
* **Giving up is time based.** Losing line of sight does not drop the target
  immediately - the NPC keeps hunting for `TargetLostTimeoutMs` and only then
  forgets. Dying or despawning drops it at once.
* **Re-engaging needs a sighting.** Acquiring a target out of Idle requires line of
  sight, not merely a live target in range. Without that, the give-up path above
  would drop the target and the acquisition step would re-adopt it in the very same
  decision, so a mob would forget and rediscover a player standing behind cover
  every `TargetLostTimeoutMs` instead of ever giving up. Getting shot bypasses this
  (`Aggro` moves straight to Chase), so you cannot hide from a mob you just hit.

### Aggro triggers

1. **Proximity.** Every `PerceptionIntervalMs` the engine scans the connected
   clients for the closest hostile, alive player character inside `AggroRadius`
   (an already engaged NPC scans `AggroRadius * 1.5` so it does not drop a target
   that just stepped out of the ring).
2. **Being shot.** `AiEngine` subscribes to `EntityDamagedEvent` and forces the
   damaged NPC onto its attacker regardless of distance. This is why you cannot
   snipe a mob from across the map for free.

The target is sticky: as long as the current target entity still exists the scan
is skipped, so a mob does not ping pong between two players standing side by side.

### Hostility

`FactionAiHostility` uses the same policy `CombatSim` applies to projectile hits:
only an explicit `Friendly` or `Self` stance is non attackable. Neutral and
unresolved faction pairs are treated as hostile on purpose - SDB faction relation
coverage is incomplete for some monster factions, and the alternative is whole
monster types that never fight back. Accord Assault (290) is Friendly to the
default player faction and will not aggro.

---

## 3. What an attack actually does

```
AiEngine.ResolveAttack
  -> IShard.Damage.ApplyDamage(target, AttackDamage, npcEntity)
       -> shields first, then health
       -> EntityDamagedEvent
            -> CharacterLifecycleService  (bleedout / death for the player)
  -> IAiAttackFeedback.OnAttack
       -> CombatSim.HitFeedback.TookDebugHit -> TookHit to scoped clients
```

It is a hitscan, not a projectile: no `ProjectileSim` trace, no ammo, no spread.
The victim gets the same `TookHit` message a weapon hit produces, so damage
numbers and the health bar behave normally. NPC attacks never crit and never
count as headshots.

Movement is applied by writing the entity position/orientation, pushing the new
pose into the physics body with `PhysicsEngine.UpdateEntity` (so the mob stays
hittable where it now stands) and broadcasting a
`AeroMessages.GSS.Character.Event.CurrentPoseUpdate` on the unreliable GSS channel
- the same message `MovementRelay` uses to show one player's movement to another.

Before stepping, a short forward ray cast checks for a wall; if the way is
blocked the NPC holds position instead of walking through it. With no collision
data loaded (`LoadMapsCollision` off) nothing can block and nothing occludes, so
every target counts as visible.

---

## 4. Commands

Available in the in-game chat (with a `\` prefix) and on the Admin channel
(without it):

| Command          | Effect                                                     |
|------------------|------------------------------------------------------------|
| `\ai` / `\ai status` | Reports whether AI is on and how many NPCs are tracked |
| `\ai on`         | Enables the engine (`enable`, `1` also work)               |
| `\ai off`        | Disables it; NPCs freeze where they stand (`disable`, `0`) |
| `\ai list`       | Lists every tracked entity id with its current state       |

The switch is per shard and not persisted.

---

## 5. Tuning

Everything is an `IAiRules` property. `AiEngine` takes an optional instance; pass
`null` to get `StandardAiRules`:

| Property             | Default | Meaning                                                          |
|----------------------|---------|------------------------------------------------------------------|
| `Enabled`            | `true`  | Master switch                                                    |
| `AggroRadius`        | `55`    | Metres at which an idle NPC notices a hostile player             |
| `AttackRange`        | `45`    | Metres at which chasing turns into attacking                     |
| `AttackRangeExit`    | `52`    | Metres at which attacking falls back to chasing                  |
| `StandoffRange`      | `4`     | Metres the NPC tries to keep from its target                     |
| `LeashRadius`        | `120`   | Metres from the spawn point before it gives up                   |
| `HomeArrivalRadius`  | `2`     | Metres from home that counts as arrived                          |
| `AttackCooldownMs`   | `1200`  | Delay between two attacks by the same NPC                        |
| `AttackDamage`       | `180`   | Flat damage per attack                                           |
| `TargetLostTimeoutMs`| `6000`  | How long an unseen target is still hunted                        |
| `PerceptionIntervalMs`| `200`  | Target scan / line of sight interval                             |
| `MovementIntervalMs` | `50`    | Movement + pose broadcast interval (20 Hz)                       |
| `DefaultMoveSpeed`   | `5`     | m/s when the monster row has no usable `normal_speed`            |
| `DefaultChaseSpeed`  | `8.5`   | m/s when the monster row has no usable `fast_speed`              |
| `MinTrustedSpeed`    | `0.25`  | Lower bound for trusting an SDB speed                            |
| `MaxTrustedSpeed`    | `35`    | Upper bound for trusting an SDB speed                            |
| `SnapToGround`       | `true`  | Pull moving NPCs onto the ground with a downward ray cast         |
| `GroundOffset`       | `0`     | Metres to add to the ground surface when snapping                |

### Movement speeds come from the database

`SdbAiMonsterStats` reads `dbcharacter::Monster.normal_speed` into the walk speed
and `fast_speed` into the chase speed. Plenty of rows are `0` and some look like
they are expressed in a different unit, so `AiSpeeds.Resolve` only trusts values
inside `[MinTrustedSpeed, MaxTrustedSpeed]` and falls back to the configured
defaults otherwise. That is what keeps a bad row from producing frozen or
teleporting mobs.

### Ground snapping

`SnapToGround` is on. The character origin sits at the feet, so `GroundOffset` is
`0`: the downward ray cast pulls the NPC's origin onto the top surface of the
static geometry and the mob walks along the terrain instead of floating or
sinking. Spawned mobs are snapped the same way before they are scoped in
(`PhysicsEngine.FindGround`), so zone entries with a placeholder `Z` of `0` land
on the ground instead of spawning deep under it.

The probe only tests static geometry (a nearby player or mob cannot be mistaken
for the ground), and the wall check ray is raised to torso height so it does not
graze the terrain the mob is standing on. When no zone collision data is loaded
(`LoadMapsCollision` off, or no map files) both probes are no-ops and movement
stays horizontal, exactly as before.

---

## 6. Known gaps

* **No animation selection.** The engine only sets the movement state
  (`0x1000` standing, `0x2004` running); there is no attack or death animation.
* **No projectiles.** Attacks are hitscan, so there is nothing to dodge and no
  tracer on screen.
* **No pathfinding.** Movement is a straight line towards the goal plus a wall
  check. A mob behind a low obstacle will stand there until the leash or the
  give-up timer fires.
* **No SDB behaviour trees.** `Monster.Behavior`, `BehaviorOffensive` and
  `BehaviorDefensive` name the live game's AI behaviour assets; PIN ignores them
  and runs one state machine for every monster type.
* **Broadcast is not scope filtered.** Pose updates go to every playing client
  like `MovementRelay` does, not just the clients the entity is scoped into.

---

## 7. Quick reference

| Action                              | Command                                  |
|-------------------------------------|------------------------------------------|
| Spawn something to fight            | `\spawn monster 1196`                    |
| Spawn a mob that will not fight back| `\spawn monster 290` (Accord, friendly)  |
| See how many NPCs are simulated     | `\ai`                                    |
| Freeze every mob                    | `\ai off`                                |
| List mobs with their current state  | `\ai list`                               |
| Check your vitals while being shot  | `\health`                                |
