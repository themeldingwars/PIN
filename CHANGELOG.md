# Changelog

## [Unreleased]

### Fixed

- Fix aiming down sights with a weapon that has an underbarrel (rifles): the client zoomed in, played the scope-in animation, then dropped the gun back to hip fire while the zoom stayed applied. `CharacterEntity.IsAltFireMode` treated `FireMode_1` (the scope/ADS state set by `UseScope`) as an alternate fire mode, so scoping in swapped the server's active weapon to the underbarrel — the server then answered with the underbarrel's weapon/spread state and the client fell out of the scoped pose. `FireMode_1` no longer selects the alt weapon; only `FireMode_0` (`SelectFireMode`) does, which is why scope-less weapons like the Raptor and the sniper rifle were unaffected. `CombatController.UseScope` also normalises `InScope` (an `sbyte`) to `0`/`1` instead of casting it directly, so a `-1` from the client no longer arrives as fire mode `255`
- Fix the first person animation flickering between states while sprinting (holding shift). `MovementRelay.CharacterMovementInput` broadcast the `CurrentPoseUpdate` to *every* playing client including the one that sent the input, so the moving player received both the authoritative `ConfirmedPoseUpdate` and a remote-avatar pose update for their own entity on every movement tick — the client kept re-applying a movement state it was already predicting locally. The pose is now only forwarded to remote clients
- Fix `MovementStateContainer.SetMovementFlag` toggling instead of clearing a flag. Clearing used `^=`, so setting `Crouch`/`Movement`/`Sprint` to `false` on a character that did not have the flag *set it*, silently corrupting the movement state used for collision shape selection and ability requirements
- Fix spawned mobs standing up instead of lying on their side, and walking on the terrain instead of floating through it. `AiVectors.OrientationFacing` built the character orientation as a 90° pitch, which put the model's up axis on the ground plane; the body orientation is a yaw-only rotation about world +Z (the model's local frame is +X right, +Y forward, +Z up, verified against the real spawn points in `StaticDB/CustomData/outpost.json`), so facing a direction is now `CreateFromAxisAngle(UnitZ, Atan2(x, y))`. Ground handling is now on by default: `EntityManager.SpawnCharacter` snaps the spawn point down onto the static geometry with the new `PhysicsEngine.FindGround` (zone entries with a placeholder `Z` of `0` no longer spawn deep under the map), and the AI's movement probe follows the terrain down slopes and ledges (`SnapToGround` defaults to `true`, probe extended to 100 m and restricted to static geometry). The wall check ray was raised to torso height so it no longer grazes the ground the mob is standing on and freezes it, and `CharacterEntity.EstimateInputVelocity` now reads the character's forward from local +Y instead of local +Z
- Fix a shard-killing `KeyNotFoundException` in `EntityManager.FlushChanges` when a deployable ability object carried more than one expired status effect. `AbilitySystem.ProcessTarget` iterates a snapshot of an entity's active effects, so when the first effect's `RemoveEffect` chain (ending in `DestroyAbilityObjectCommand`) removed the entity from the shard, the system kept processing the entity's remaining effects from that snapshot and flushed network changes for an entity that was no longer registered — `ProcessTarget` now stops processing an entity as soon as it leaves the shard. `EntityManager` no longer throws for operations on an entity that was removed mid-flight either: `FlushViewChangesToScoped` treats an unknown entity as having no scoped-in players (no-op), `ScopeIn`/`ScopeOut` and the periodic scope check skip removed entities, and `OnRemovedEntity` iterates a copy of the scoped-player set (it previously mutated the set it was enumerating through `ScopeOut`, an `InvalidOperationException` waiting for any removal that a player could see)
- Add `AbilitySystemTests` covering both crash paths (an entity destroyed by its own effect removal, and flushing changes of an already-removed entity)

- Fix `GameServer terminated: CodeBase is not supported on assemblies loaded from a single-file bundle`, which killed every published `GameServer.exe` on startup. `ConfigurationManager` locates `App.config` through `Assembly.CodeBase`, an API that is unsupported inside a single-file bundle, so the very first settings lookup threw before `GameServer.config.json` was ever read — setting the Firefall paths by hand could not help. The `appSettings` block is now parsed straight from disk by the new `AppConfigFile` (`GameServer.dll.config` / `GameServer.exe.config` / `App.config`, next to the executable or in the working directory), Serilog is configured from those same values instead of `ReadFrom.AppSettings()`, and `System.Configuration.ConfigurationManager` is gone from the GameServer dependency closure
- Ship `App.config` next to the published `GameServer.exe` (as `GameServer.dll.config`) and read it from there, so `Port`, `ZoneId`, `ClientVersion`, `GrpcChannelAddress` and the `serilog:` keys are configurable in the release build again
- Explain an unsupported single-file operation as a build problem in the startup error instead of pointing the user at their Firefall paths
- Assert in the Windows CI and release smoke tests that the published `GameServer.exe` gets past configuration loading to opening the StaticDB, and fail on any `CodeBase` / single-file bundle error — the previous smoke test accepted that crash as a healthy startup failure

- Publish GameServer as a framework-dependent single-file executable with `Bitter` and the rest of its managed dependency closure embedded, preventing missing-assembly startup failures even if loose release files are omitted or separated.
- Report a missing managed runtime assembly as an incomplete server installation instead of directing the user to the Firefall-path configuration.
- Pin the Bitter submodule build to version 1.0.0 in `Directory.Build.props` (what FauFau 1.5.1 was compiled against): the PIN product version stamp produced a skewed assembly that failed to load from the single-file bundle at runtime.
- Boot-test the published `GameServer.exe` on Windows CI and the extracted release archive with a dummy StaticDB file, forcing the `Bitter` assembly load so an unbundled dependency fails the build instead of the user.
- Fix startup and config messages that still pointed at `GameServer.dll` after the single-file switch; the incomplete-installation error now tells stale installs to download the latest release.

## [0.2.0] - 2026-09-05

This is the first release that ships the server side NPC AI, the static database
tooling and spawn catalogue, a working health system, real ability cooldowns, and a
character that actually looks the same in the selection screen as it does in game.
It consolidates all 27 pull requests merged since the project was picked up again —
`v0.1.0` covered PRs #1–#3, everything from PR #4 onwards is new in `v0.2.0`.

See [Pull requests in this release](#pull-requests-in-v020) at the bottom for the
itemised list.

### Added

#### NPC AI ([#27](https://github.com/kasperfriend/PIN/pull/27))

- Implement server side NPC AI, replacing the empty `AIEngine` stub that did nothing on tick. Every character spawned through `EntityManager.SpawnCharacter` now gets a brain that runs an Idle / Chase / Attack / Return / Dead state machine: it scans for the closest hostile player inside an aggro radius (55 m), walks towards them at the monster row's `fast_speed`, and once they are inside 45 m with a clear line of sight applies flat damage through `DamageSystem` and sends the regular `TookHit` feedback. Being shot aggros the mob on its attacker regardless of distance, losing line of sight keeps it hunting for 6 s, and dragging it more than 120 m from its spawn point makes it drop the target and walk home. Movement is pushed into the physics body so mobs stay hittable where they now stand and broadcast as `CurrentPoseUpdate`, the same message that relays player movement. New `Systems/Ai/` namespace, documented in `Docs/NPC_AI.md`
- Add the `ai` command (chat with `\`, and on the Admin channel): `status` reports how many NPCs are simulated, `on`/`off` toggle the engine per shard, `list` dumps every tracked entity with its current state
- Split NPC AI into a pure decision half (`AiBrain`, fed an `AiPerception` snapshot, no shard/entity/physics dependency) and a shard integration half (`AiEngine`), with the tunables behind `IAiRules`, hostility behind `IAiHostility`, monster speeds behind `IAiMonsterStats` and hit feedback behind `IAiAttackFeedback` so every part is replaceable in tests
- Add `Docs/NPC_AI.md` covering the state machine, aggro triggers, the attack path, every tunable and the known gaps
- Add `AiBrainTests`, `AiEngineTests`, `AiSpeedsTests`, `AiVectorsTests` and `StandardAiRulesTests` to `GameServer.Tests` (state transitions, cooldowns, leash, aggro on damage, death, the kill switch, speed resolution and the character facing maths)

#### Static database tooling and spawn catalogue ([#20](https://github.com/kasperfriend/PIN/pull/20), [#21](https://github.com/kasperfriend/PIN/pull/21), [#22](https://github.com/kasperfriend/PIN/pull/22))

- Add `Docs/SpawnReference/`, the full catalog of everything PIN can spawn: one Markdown spreadsheet per kind (`MOBS.md` 3,109 rows, `DEPLOYABLES.md` 3,902, `VEHICLES.md` 173, `CARRYABLES.md` 105, `TURRETS.md` 107 - 7,396 rows in total) where every row lists its resolved name, faction/category/class and the exact `\spawn <kind> <id>` command, plus an index document with the command syntax, kind aliases, the faction table and the monster scaling table
- Add `Docs/SpawnReference/csv/`, the same rows as spreadsheets with **every** raw SDB column plus resolved foreign keys (chassis, weapons, loot tables, deployable category/function, vehicle class, granted ability, titles) for filtering in Excel/LibreOffice or diffing between builds
- Add `Tools/SdbDump/spawn_reference.py`, the generator for that folder: it imports the `sdb_dump.py` decoder, joins the spawnable tables against the lookup tables and decrypts only the `dblocalization::LocalizedText` rows those tables reference, so a full regeneration takes ~30 s and needs nothing but Python 3
- Deployables are catalogued for the first time (all 3,902 rows, 2,088 of them named, grouped by `DeployableCategory` / `DeployableFunction`)
- Add `Tools/SdbDump`, a standalone decoder for `clientdb.sd2` (port of the FauFau StaticDB format logic PIN loads data through, verified by a round-trip self-test), and `Docs/MOBS_AND_NPCS.md` cataloging every mob/NPC in the database (build `prod-1962`: 3,109 monster rows, 1,772 of them named, grouped by faction) plus turrets and the per-level scaling table
- Add `spawnables` and `coverage` subcommands to `Tools/SdbDump`, mirroring the in-game catalog offline and reporting how much of `clientdb.sd2` PIN's loader actually reads (240 of 575 tables, 53.5% of rows for build `prod-1962`)
- Load `dblocalization::LocalizedText` (175,293 rows) and `dbcharacter::MonsterScaling` from the static database, exposed as `SDBInterface.GetLocalizedString(id)` and `SDBInterface.GetMonsterScaling(level)`, so the server can resolve display names instead of only ids
- Add a generic static-database spawn command available both in chat and on the Admin channel: `spawn <monster|deployable|vehicle|carryable|turret> <id|name> [<x> <y> <z>]`, resolving rows by numeric id **or** by (multi-word, case-insensitive) localized name, backed by the new `StaticDB/SDBCatalog.cs` and `Systems/Spawning/SDBSpawner.cs`. Turret spawning from a command is new; monsters/deployables/vehicles/carryables keep their existing typed commands too
- Add `sdb` (browse/search the ~7,400 spawnable rows) and `sdbinfo` (dump a single row's gameplay fields) commands, both in chat and on the Admin channel
- Add `Docs/STATIC_DATABASE.md` documenting the `.sd2` format, PIN's table coverage, the unread tables that are the obvious next targets, and the new spawn/browse commands; extend `Docs/MOBS_AND_NPCS.md` with the vehicle and carryable catalogs

#### Health system ([#18](https://github.com/kasperfriend/PIN/pull/18))

- Implement fall damage: `FallDamageSystem` tracks each player's airborne samples from the client authoritative movement inputs and applies damage on landing based on the fastest downward speed of the fall (water landings, thruster/glider use, knockdown falls and the `immune_falldamage` combat flag negate it; lethal at very high impact speeds)
- Add a `GameServer.Tests` xUnit project covering `FallDamageMath`, `FallDamageSystem`, `DamageSystem`, `CharacterLifecycleService` and the `CharacterEntity` vital clamping; CI now runs `dotnet test`
- Add player facing health debug commands to the in-game chat: `\health`, `\hurt <amount>`, `\heal <amount>`, `\fall <speed>`, `\down`, `\revive`, `\kill`, `\respawn`, so the health system can be exercised without enemies
- Add `Docs/HEALTH_SYSTEM.md` documenting the damage pipeline, the fall damage rules and the in-game testing commands

#### Characters, battleframes and persistence ([#11](https://github.com/kasperfriend/PIN/pull/11), [#13](https://github.com/kasperfriend/PIN/pull/13))

- Implement the server side of the gRPC `GameServerAPI` (`WebHost.GameServerApi`, hosted by `WebHostManager` on port 5201), so the GameServer can actually load character data instead of always falling back
- Persist characters to `characters.json`, shared by the character selection screen and the GameServer
- Persist the battleframe picked in-game, so switching frames survives a relogin and shows up in the character selection screen
- Add `Shared.Common.DefaultCharacterTemplate`, a single source of truth for the default character's frame, gender, race and visuals, so the selection screen and the in-game character can no longer drift apart
- Add `Docs/CHARACTERS_AND_BATTLEFRAMES.md`, a wiki for `characters.json` (GUID scheme, 38 seeded zones, full JSON structure, known battleframe SDB ids, persistence flow, editing and troubleshooting recipes) and `Docs/characters.example.json`, a minimal working example
- Spawn zone NPCs from StaticDB custom data (`character_spawn.json`, zones 12 & 1003) with per-spawn `MaxHealth`/`MaxShields`, replacing the hardcoded `factionTest` block ([#2](https://github.com/kasperfriend/PIN/pull/2))
- Add the `\npc` chat command (aliases `character`, `monster`, `spawn_npc`, `spawn_character`, `spawn_monster`) to spawn a character at the player or at `x y z` ([#2](https://github.com/kasperfriend/PIN/pull/2))

#### Abilities and the aptitude runtime ([#12](https://github.com/kasperfriend/PIN/pull/12), [#14](https://github.com/kasperfriend/PIN/pull/14), [#23](https://github.com/kasperfriend/PIN/pull/23), [#24](https://github.com/kasperfriend/PIN/pull/24), [#25](https://github.com/kasperfriend/PIN/pull/25), [#26](https://github.com/kasperfriend/PIN/pull/26))

- Implement the `ResetCooldowns` aptitude command (type 209, "Cooldown - Reset Abilities", 18 chain instances in build `prod-1962`): it loaded as a placeholder, so items/abilities that reset ability cooldowns did nothing; it now clears every tracked local, category and global cooldown of the current targets (falling back to the entity running the chain), and the client timers refresh with the next ability-activation response since that payload is built after the chain ran
- Implement the `TargetByExists` aptitude command (type 134, "Target - Filter Existing Objects", 135 chain instances in build `prod-1962`): it loaded as a placeholder that kept every target, so chains holding a target over time (effect update/duration chains, deployables, called-down vehicles, NPC chains) went on acting on entities that had already despawned; it now keeps only the targets still registered with the shard, moving the pre-filter list to the former targets like the other target filters
- Implement the `RequireInRange` aptitude command (type 81, "Requirement - In Range"): it loaded as an always-succeed placeholder, so chains gated on staying near their target (tethered buffs, beam-style heal/repair effects, interaction and NPC follow-up chains) kept running no matter how far the target ran off; it now fails when any current target is further than the def's `Range` from the entity running the chain, honours the `Negate` flag, leaves the target list untouched and keeps succeeding when the chain has no targets
- Implement the `TargetDifference` aptitude command (type 101, "Target - Difference", 21 chain nodes in build `prod-1962`): it loaded as a placeholder that only shuffled the two target lists around without ever subtracting anything, so both chain shapes that use it kept their full target list. It now drops every target that appears in both the current and the former list, honouring `SwapCurrentFormer` (subtract the current list from the former one instead, for the layered `TargetConeAE` blasts that need the ring between an outer and an inner volume) and `ReplaceFormer` (keep the unfiltered list as the former one). Periodic area chains that pop the previously hit set into the former list therefore act on the entities that *entered* the area since the last tick instead of re-applying to everybody standing in it, and ring blasts no longer hit their inner volume once per ring
- Add `AbilityState` / `AbilitySystem`, the per-entity tracker of ability cooldowns (local / category / global) and of the jetpack energy pool that the aptitude runtime had no notion of before
- Add the `\abilityinfo` chat command (aliases `\abi`, `\ability`, `\aptitudeinfo`): dumps every ability slot of the current loadout as `module id -> ability id -> chain`, and for each chain node prints the command type, its environment (`client`/`both`/`server`) and whether the runtime resolves it to a real command, an unimplemented placeholder or a client-only no-op, plus the player's server `AbilityState`
- Add `AbilityStateTests`, `TargetByExistsCommandTests`, `RequireInRangeCommandTests` and `TargetDifferenceCommandTests` to `GameServer.Tests`

#### Configuration and Firefall install detection ([#5](https://github.com/kasperfriend/PIN/pull/5), [#6](https://github.com/kasperfriend/PIN/pull/6), [#9](https://github.com/kasperfriend/PIN/pull/9))

- Add `GameServer.config.json`, a user-editable config read next to `GameServer.dll`, holding `StaticDBPath`, `MapsPath`, `AssetDBPath` and `CachePath` — the hardcoded Steam paths are gone from `GameServerSettings` and `App.Default.config`, and the file is gitignored so machine-specific paths are not committed
- Auto-detect the Firefall installation and populate `GameServer.config.json` paths on startup (Steam registry location, Steam default install, every library in `steamapps\libraryfolders.vdf`, standalone installs under `Program Files`, or the `PIN_FIREFALL_PATH` / `PIN_STEAM_PATH` overrides). Existing user values are never overwritten and the scan is skipped entirely when all paths are set
- Print a readable startup error instead of an unhandled Autofac exception when no Firefall installation is found and `StaticDBPath` is not configured

#### Build and release tooling ([#2](https://github.com/kasperfriend/PIN/pull/2), [#4](https://github.com/kasperfriend/PIN/pull/4))

- Add the manual **Release** GitHub Actions workflow (`.github/workflows/release.yml`): publishes GameServer, MatrixServer and WebHostManager for Windows x64, bundles `Start.cmd` / `README.md` / `CHANGELOG.md` into `PIN-<tag>-win-x64.zip`, and creates or updates the matching GitHub release. The patched `FirefallClient.exe` is **not** built by CI and must be attached to each release manually (see README step 6)
- Add support for StyleCop & .NET Analyzers
- Use RIN via gRPC for the player management
- Add many items to the game
  - Deployables
  - Battlestation
  - Vehicles
  - Gliders
  - Abilities
  - Thumbers
  - Turrets
  - Melding Repulsor
- Add Bepu as physics engine

### Changed

- `Tools/SdbDump`: `spawnables` now decrypts only the localized strings the printed rows reference instead of building the whole 175k-row `dblocalization::LocalizedText` map, which takes the run from ~15 min down to ~15 s (output is byte-identical)
- `SDBInterface` now exposes resolved localized names, so commands and logs say *Aranha Queen* instead of *2435*
- Update build pipeline to support .NET 8 & 9 and the latest macOS version
- Update most dependencies

### Fixed

#### Abilities

- Fix activated abilities (Raptor and every other battleframe) staying purely cosmetic: `InstantActivation`, the chain node that carries each ability's cooldown configuration, was an empty stub, so no cooldown was ever started, the client got an empty cooldown payload and the ability could be re-pressed forever
- Start activation cooldowns only after the whole ability chain succeeded (queued by `InstantActivation`, committed by `AbilitySystem.HandleActivateAbility`), so an ability that fails a later requirement does not go on cooldown
- Track the aptitude cooldown category per ability (learned from its activation command) instead of using `AbilityModule.UiCategory`, which is a UI grouping and never matched a real cooldown category
- Report category cooldowns to the client (`ActiveCooldowns_Group2`) and express the global cooldown window in shard time, so the client-side ability timers no longer jump
- Wire the aptitude register pipeline so data-driven amounts are computed correctly: `LoadRegisterFromStat` (aptitude stat modifiers, e.g. energy/cooldown), `LoadRegisterFromModulePower` (ability module power rating), and `PushRegister`/`PopRegister`/`PeekRegister` are now implemented and resolved in `Factory`; chains that multiply an SDB amount by a loaded value no longer collapse to 0
- Mirror the client recharge model in the server `AbilityState`: regeneration waits `EnergyParams.Delay` after the last spend, an overcharged (negative) pool keeps recharging back through zero, and `EnergyToDamage` converts the actual tracked pool instead of assuming a full one
- Keep ability energy client-simulated like the live game: abilities do not cost (or get gated by) the regular energy pool - only the jetpack/thrust drains it. Client-environment aptitude energy commands (`RequireEnergy`, `ConsumeEnergy`, `ConsumeEnergyOverTime`, `RequireEnergyByRange`) load as no-ops on the server again, and the hardcoded per-activation fallback costs were removed
- Derive the replicated jetpack energy parameters (`EnergyParams`: max, recharge rate, recharge delay) from the equipped battleframe's SDB record whenever a loadout is applied
- Log aptitude chain nodes that are still unimplemented server-side, so an ability that plays its animation but does nothing can be traced to the exact placeholder command
- Fix Charge leaving the camera look locked after the server cleared the effect: `CharacterEntity.SetStatusEffect` / `ClearStatusEffect` now also write the owner-private `Character_LocalEffectsController` slots, and `BaseAptitudeEntity.AddEffect` sends the real effect stack count ([#10](https://github.com/kasperfriend/PIN/pull/10))

#### Combat, health and damage

- Fix the first hit after a respawn instantly downing the character again: `NetworkPlayer.Respawn` wrote the reset health only into the replicated controller props while the entity stayed at 0 HP; it now resets vitals through `CharacterEntity.SetMaxHealth`/`SetCurrentShields` so server state and client view agree
- `DamageSystem` no longer damages or heals characters that are not in the `Living` state (corpses and bleeding out characters were fully damageable before), and `EntityDamagedEvent`/`EntityHealedEvent` are only published when the damage/heal actually applied
- `CharacterLifecycleService` only transitions out of the `Living` state now, so stray damage events can no longer skip the bleedout phase or double-fire death transitions
- Respawn and teleport reset the fall damage tracker, so stale fall speeds cannot deal damage at the destination
- `CombatSim.OnProjectileHit` no longer dereferences a null target or source when the hit cannot be resolved from the shard, and drops hits where the source's stance towards the target is `Friendly` or `Self` ([#1](https://github.com/kasperfriend/PIN/pull/1))
- `EntityManager.SendToScoped` no longer throws `KeyNotFoundException` for entities that were never scoped (a latent crash, reachable through `HitFeedback`)

#### Characters and login

- Fix the character selection screen and the in-game character not matching (selection showed a female Raptor while the game spawned a male Mammoth, because the two were built from separate hardcoded blobs and the gRPC lookup that was meant to reconcile them had no server implementation)
- Decode the zone id from the character guid on **both** the remote and the fallback login path - the 38 "characters" in the selection list are really zones, and only decoding it on the fallback path meant a working gRPC call would have dropped every entry into the same zone

#### Startup, configuration and build

- Fix Firefall auto-detection failing for every Steam library listed in `libraryfolders.vdf` (Steam stores the library root folder such as `D:\SteamLibrary`, not the `steamapps` folder, so all entries were rejected)
- Find Steam installations outside `Program Files` through the Windows registry, and standalone Firefall installs under `Program Files`
- Stop empty values in `GameServer.config.json` from overriding paths configured in the legacy `App.config`
- Fix WebHostManager startup crash (missing `Serilog.Enrichers.Context` and other Serilog extension assemblies when the servers are published into a single folder) by referencing the shared Serilog packages ([#6](https://github.com/kasperfriend/PIN/pull/6))
- Fix the Release workflow passing an empty `--prerelease` argument to `gh release create`, which gh treated as an asset glob and made release creation fail with `no matches found for ''` ([#4](https://github.com/kasperfriend/PIN/pull/4))
- Fix `CS1736` in `EntityManager.SpawnCharacter` (`Quaternion.Identity` is not a compile-time constant, so it cannot be a default parameter value) ([#3](https://github.com/kasperfriend/PIN/pull/3))
- Fix `CS0718` / `CS1503` build errors in `FirefallInstallLocator` (static class used as a `ForContext<T>()` type argument, then a bare-string `ForContext` overload that does not exist) ([#7](https://github.com/kasperfriend/PIN/pull/7), [#8](https://github.com/kasperfriend/PIN/pull/8))
- Make the server handling code more robust
- Many improvements in the entity definitions

### Pull requests in v0.2.0

| PR | Title | Merged |
|---|---|---|
| [#1](https://github.com/kasperfriend/PIN/pull/1) | Combat: validate hostility on projectile hits and null-guard damage paths | 2026-09-03 |
| [#2](https://github.com/kasperfriend/PIN/pull/2) | Character spawns, mob spawning, combat validation & release workflow | 2026-09-03 |
| [#3](https://github.com/kasperfriend/PIN/pull/3) | Fix CS1736 build error in `EntityManager.SpawnCharacter` | 2026-09-03 |
| [#4](https://github.com/kasperfriend/PIN/pull/4) | fix(release): empty `--prerelease` arg broke release creation | 2026-09-03 |
| [#5](https://github.com/kasperfriend/PIN/pull/5) | Move Firefall data paths into `GameServer.config.json` | 2026-09-03 |
| [#6](https://github.com/kasperfriend/PIN/pull/6) | Auto-detect Firefall install paths and fix WebHostManager Serilog crash | 2026-09-03 |
| [#7](https://github.com/kasperfriend/PIN/pull/7) | Fix CS0718 build error: static type used as type argument in `FirefallInstallLocator` | 2026-09-03 |
| [#8](https://github.com/kasperfriend/PIN/pull/8) | Fix GameServer build: correct Serilog `ForContext` and StyleCop warnings | 2026-09-03 |
| [#9](https://github.com/kasperfriend/PIN/pull/9) | Fix GameServer startup crash when Firefall auto-detection fails | 2026-09-03 |
| [#10](https://github.com/kasperfriend/PIN/pull/10) | Fix Charge ability local effect replication | 2026-09-03 |
| [#11](https://github.com/kasperfriend/PIN/pull/11) | Fix character selection not matching the in-game character | 2026-09-03 |
| [#12](https://github.com/kasperfriend/PIN/pull/12) | Fix Raptor abilities end to end | 2026-09-03 |
| [#13](https://github.com/kasperfriend/PIN/pull/13) | docs: add `characters.json` and battleframes wiki guide | 2026-09-03 |
| [#14](https://github.com/kasperfriend/PIN/pull/14) | Diagnose why Raptor abilities still behave cosmetic-only in game | 2026-09-03 |
| [#15](https://github.com/kasperfriend/PIN/pull/15) | Fix activated abilities never applying cooldowns | 2026-09-03 |
| [#16](https://github.com/kasperfriend/PIN/pull/16) | Implement ability energy costs with client-matching recharge model | 2026-09-03 |
| [#17](https://github.com/kasperfriend/PIN/pull/17) | Fix server ability energy state synchronization | 2026-09-03 |
| [#18](https://github.com/kasperfriend/PIN/pull/18) | Working health system: fall damage, respawn vitals fix, health debug commands, unit tests | 2026-09-03 |
| [#19](https://github.com/kasperfriend/PIN/pull/19) | Implement authoritative ability energy costs | 2026-09-03 |
| [#20](https://github.com/kasperfriend/PIN/pull/20) | Revert ability energy costs (abilities are jetpack-only for energy) + SdbDump tool and mobs/NPCs doc | 2026-09-04 |
| [#21](https://github.com/kasperfriend/PIN/pull/21) | Static database: spawnable catalog, generic spawn/sdb commands and docs | 2026-09-05 |
| [#22](https://github.com/kasperfriend/PIN/pull/22) | Docs: full spawnable reference (`Docs/SpawnReference`) with the exact command per row | 2026-09-05 |
| [#23](https://github.com/kasperfriend/PIN/pull/23) | Implement the `ResetCooldowns` aptitude command (first point off the Todo list) | 2026-09-05 |
| [#24](https://github.com/kasperfriend/PIN/pull/24) | Implement the `TargetByExists` aptitude command (next point off the Todo list) | 2026-09-05 |
| [#25](https://github.com/kasperfriend/PIN/pull/25) | Implement the `RequireInRange` aptitude command (next point off the Todo list) | 2026-09-05 |
| [#26](https://github.com/kasperfriend/PIN/pull/26) | Implement the `TargetDifference` aptitude command (next point off the Todo list) | 2026-09-05 |
| [#27](https://github.com/kasperfriend/PIN/pull/27) | Implement server side NPC AI (biggest point off the Todo list) | 2026-09-05 |

### Known limitations

- NPC movement is horizontal only: `SnapToGround` is implemented but **off by default**, because nothing in the repo pins down whether a character origin sits at the feet or at the centre of the body. It is a one-line rule flip plus a `GroundOffset` once that has been confirmed in game (see `Docs/NPC_AI.md`)
- NPC AI tuning (the 180 damage / 1.2 s attack cadence and the `CurrentPoseUpdate` broadcast rate) has not been validated against a running client — it needs in-game play, not CI
- ~180 aptitude commands are still `Todo/` placeholders. `Factory` now logs each one it falls back on, so `\abilityinfo` shows exactly which command a cosmetic ability is waiting for
- 14 identified `dbcharacter` tables (`MonsterVisualOption`, `MonsterMood`, `MonsterItemTags`, `MonsterTitle`, `TurretWeapon`, `VoiceSet`, …) are still unread; they are listed in `Docs/STATIC_DATABASE.md` as the next targets
- The patched `FirefallClient.exe` is not built by CI and must be attached to each release manually (README step 6)

## [1.2.0] - 2023-06-02

### Added

- Add support for calling down a LGV
- Add documentation to explain the architecture
- Use Autofac for dependency injection in UdpHosts
- Add basic endpoint for character creation handling
- Add or extend the API endpoints
  - api/v2/accounts/current/status
  - api/v2/accounts/character_slots
  - api/v3/characters/{character_id}/garage_slots
  - api/v3/ui_actions
  - api/v1/characters/{character_id}/data
  - api/v3/characters/{character_id}/inventories/bag
  - api/v3/characters/{character_id}/inventories/gear/items
  - api/v1/zones/queue_ids
  - api/v2/zone_settings
  - api/v1/item_display_attributes
  - api/v1/market_categories
  - api/v1/characters/validate_name
  - api/v3/characters/{characterId}/titles
  - api/v3/characters/{characterId}/garage_slots/{frameId}/perks
  - ...and more
- Use AeroMessages as submodule instead of a binary reference
- Use range indexer
- Modernize code base with support from Rider auto format and clean up
- Handling of the Steam user id. Currently, it is only held internally and not persisted.
- Basic GitHub Action to ensure continuous integration
- Individual .bat files for each game service pointing directly to the respective `bin\debug` folder.
- Configuration for the game server. You can edit the `App.config` file in the `GameServer` project root for local settings and add working defaults in `App.Default.config`.
  `App.config` will be generated from `App.Default.config` if not present before build.
  Currently, the only option present is the Serilog log level.
- CLI options parsing. Currently, the only option is the log level. Specifying wrong options will not stop the server from starting.
- 404 handling to the web server pipeline which prints the contents of 404-producing requests as warnings to see what's missing.
- ClientEventController with a corresponding endpoint to receive the events from the client. Currently, only the client uptime seems to be posted on exit of the game

### Changed

- Jets rendering correctly
- Use AeroMessages for nearly all packets
- Clean up some of the code flow, for easier understanding
- Use long speaking names for variables
- Started transition to AeroMessages, this is an incremental process
- Replace SharedAssemblyInfo with a targets file
- Update to .NET 6
- Changed 'missing MSGid' logging to include the details (1st message) in log level warning instead of verbose
- Update documentation regarding the usage of web hosts
- Turn Firefall specific location finder to a common location provider

### Fixed

- Fix IndexOutOfRangeException being thrown by the Matrix server
- Fix string deserialization and corrected Matrix Login packet

## [1.1.0] - 2021-10-09

### Added

- Characters for each available zone with usable spawn location

## [1.0.0] - 2021-10-06

### Added

- MatrixServer to handle client connection establishment and hand off to GameServer
  - Supports all five packets: ABRT, HEHE, HUGG, KISS, POKE
- GameServer to handle map zoning and basic character movement
  - Zone into New Eden
  - Spawn on a Watch Tower
  - Have a pre-defined set of Visuals
  - Use your Primary and Secondary Weapon (sometimes it doesn't work)
  - Run and sprint around the whole map
- WebHostManager to deal with standard web requests from the client through different WebHosts
  - Handle login requests via hardcoded Oracle ticket
  - Provide hardcoded account details
  - Serve the necessary Host Information
  - Return static assets when provided by the user
