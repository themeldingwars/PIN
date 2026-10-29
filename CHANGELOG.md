# Changelog

## [2.0.0] - Unreleased

PIN went from "walk around New Eden" to an actual game server: the server now simulates weapons, physics, abilities and NPCs, players gain XP and loot, and everything is saved through RIN.
At the same time the way PIN is deployed changed, which is why this is a new major version.
If you're coming from 1.2, keep in mind:

- PIN only runs the Matrix and game server, all the HTTP traffic of the client goes to [RIN.WebAPI](https://github.com/themeldingwars/RIN.WebAPI), and the GameServer needs RIN.InternalAPI for the login
- [Calldown](https://github.com/themeldingwars/Calldown) is the recommended way to run the whole stack, it starts the database, RIN, PIN and the client with one `aspire run`
- The configuration moved from `App.config` to `appsettings.json`
- PIN needs the .NET 9 runtime and the path to your Firefall install, it reads the static DB, the asset DB and the maps from there

### Added

- Combat
  - Simulate the weapons on the server from the SDB weapon templates: fire modes, rate of fire, bursts, charge-up, spread, ammo and projectile travel
  - Detect hits against the world and the hit shapes of characters, deployables and vehicles
  - Apply damage with damage types and the damage response of the target, show damage numbers and hit sounds through hit feedback
  - Let characters die with the matching gib visuals and respawn
  - Decide who can fight whom through faction hostility, between players, NPCs, deployables and turrets
  - Fire projectiles from abilities and deal area damage in a radius, a sphere or a cone
- NPCs
  - Spawn any monster of the SDB with its model, stats, weapons and abilities
  - Run the AI behaviors of the SDB: idle, wander, aggro on hostiles, keep a combat distance, use abilities and fire with line of sight
  - Navigate on a navmesh baked from the zone's collision, with crowd steering so NPCs don't walk through each other
  - Let NPC gunners man the Engineer turrets, they scan like on the live servers, track hostiles and fire
- Progression and loot
  - Award XP for kills, scaled by the level and XP modifiers, and level up battleframes along the SDB's progression curve
  - Drop loot from the loot tables of monsters and destroyed deployables, collected loot goes into the inventory
  - Grant and persist unlocks such as battleframes and titles
- Persistence through RIN.InternalAPI over gRPC
  - Validate logins and load the character from RIN
  - Save the inventory, resources, loadouts, unlocks and progression
  - Save the session, i.e. the zone and position, when a player logs out or disconnects
  - Apply changes that come in from the web API, e.g. army and inventory changes, while the player is online
- Inventory and loadouts
  - Equip items, switch battleframes and loadouts
  - Use gliders and vehicles from the hotkeys and call down items from the inventory
- Abilities
  - Load every ability command of the SDB, plus the server-only data the client never had
  - Implement a large part of the commands: effects with durations, update chains, stat modifiers, registers, requirements, targeting, impacts and interactions
- Content
  - Deployables, battlestations, vehicles with their turrets and abilities, gliders, thumpers, turrets, carryables, terminals, outposts and meldings
  - The Melding Repulsor
  - Thumpers and the beta LGV races as encounters
  - World deployables from the 1962 replays in New Eden, Diamond Head, Sertão, Refinery TDM, Crash Down, Icebreaker and Defense of Dredge
- World
  - Physics with BepuPhysics2 on the zone's terrain and objects, read directly from the client's map files
  - The CollisionGenerator tool, it bakes the collision cache and the navmesh ahead of time so the GameServer starts quickly
  - Run several zones in one GameServer, with transfers between them that wait for RIN to save the character
  - Only send clients the entities around them, scoped by distance
  - Interpolate and relay player movement to the other players
- Zone chat and emotes
- Admin commands for testing, `/help` lists them all, e.g.:
  - teleport, change the zone, spectate
  - spawn NPCs, vehicles, deployables and carryables, and let NPCs follow, patrol or wander
  - give XP, set the level, drop and dump loot
  - apply and remove effects, heal, hurt, revive or kill yourself
- Operations
  - Optional health endpoints for both UDP servers
  - Export logs over OpenTelemetry when configured, besides the console, a file and Seq, with log levels per system
  - Override any setting from the command line
  - A physics debug window that shows what the server simulates
  - The MinimalSDB tool, it writes an SDB with only the tables PIN reads
- StyleCop and the .NET analyzers for development

### Changed

- PIN only runs the Matrix and game server, RIN.WebAPI serves the web API and the login
- Reject the login when RIN.InternalAPI can't be reached, `AllowHardcodedCharacterData` brings back the hardcoded character
- Switch the configuration from `App.config` to `appsettings.json`, local overrides go into `appsettings.Local.json`
- Make the ports of both UDP servers and the paths to the Firefall install configurable
- Use the 1962 field names and layouts of AeroMessages and send every packet through AeroMessages
- Batch outgoing messages into packets and send the GSS messages in the order the client expects
- Let the shard thread wait instead of spinning, an idle server no longer burns a CPU core
- Read the SDB, zones and chunks through FauFau 2.0, and use Bitter as a NuGet package
- Target .NET 9 and update all dependencies

### Removed

- Remove the WebHosts and `WebHostManager`, RIN.WebAPI serves the web API now
- Remove the old hand-written packets
- Drop the builds for .NET 6 and 7

### Fixed

- Hand out random 32-bit socket ids, two clients could get the same one out of 256
- Drop clients that send nothing for `ClientTimeoutSeconds` (60 s) and forget closed connections
- Resend reliable messages the client didn't acknowledge in time and limit how many can be in flight
- Ignore datagrams for a connection that come from another address
- Ignore the connection resets Windows reports for clients that are gone
- Start effects at the time they were applied, so the client's timers line up with the server
- Spawn new characters at the correct default location
- Fix the client error when swapping weapons
- Fix wrong eye colors and visuals
- Fix several crashes, e.g. on unknown packets, stat modifiers, ray casts before an entity has a body, deployables without collision and entities removed while queued
- Run delayed abilities and RIN events on the shard thread instead of racing it

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
