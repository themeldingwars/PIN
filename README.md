# PIN - Pirate Intelligence Network

The Accord may think their Shared Intelligence Network is unique and impenetrable, but not everyone agrees with their restricted access and constant surveillance. That's why PIN, the Pirate Intelligence Network, has been created.

*Fight the Accord - Kill the Chosen*

https://user-images.githubusercontent.com/920861/134824107-03e9f99c-b420-47c7-b742-efe68967161c.mp4

## Usage

**Note:** If you want to play around with the configuration, see the Development section below

1. Install Firefall via Steam (paste `steam://install/227700` into address bar of web browser)
2. Edit the `firefall.ini` located in `steamapps\common\Firefall`
3. Add content from below
4. Download and extract the [latest PIN release](https://github.com/themeldingwars/PIN/releases/latest) as a complete archive. Keep `GameServer.exe` and every adjacent file together in the extracted folder. `GameServer.exe` is a single-file build: if you see a `GameServer.dll` next to it, that folder holds an outdated release — delete it and re-extract the latest archive.
5. On its first launch, GameServer automatically finds a normal Steam Firefall installation and writes the paths to `GameServer.config.json`. If it cannot find your copy, set `StaticDBPath`, `MapsPath`, and `AssetDBPath` in that file before trying again; see [GameServer config](#gameserver-config).
6. Make a backup copy of the original `FirefallClient.exe` in `Firefall\system\bin`
7. Replace the `FirefallClient.exe` with the patched `FirefallClient.exe` from the PIN release
   - The patched client is **not built by CI** (it is an external binary). Attach it
     manually to the release's **Assets** on the GitHub release page, then download it
     from there. A release produced by CI contains the three servers only.
8. Make sure the [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) is installed
9. Trust self-signed development certificates by running `dotnet dev-certs https --trust`
10. Start all three applications:
   - GameServer
   - MatrixServer
   - WebHostManager
11. Start Firefall
12. Login to the server:
    - If Steam auto login has been enabled, you will directly be navigated to the character selection screen
    - Otherwise, leave the login fields blank or enter anything you want and click "Login"
13. Load into the game by pressing the "Enter World" button

### Character persistence

Characters are stored in `characters.json`, created next to the `WebHostManager`
binary on first run and seeded with the built-in entries. The same store backs
both the character selection screen and the GRPC `GameServerAPI` the GameServer
calls on login, so the character you pick is the character you spawn as.

The GRPC endpoint is hosted by `WebHostManager` on port `5201` (plain HTTP/2, no
TLS) and must match `GrpcChannelAddress` in the GameServer settings. Both are
configurable:

```json
"Firefall": {
  "GameServerApi": {
    "Port": 5201,
    "CharacterStorePath": ""
  }
}
```

Leave `CharacterStorePath` empty to use the default location. If `WebHostManager`
is not running, the GameServer logs a warning and falls back to a hardcoded
character.

> **Full guide:** See [`Docs/CHARACTERS_AND_BATTLEFRAMES.md`](Docs/CHARACTERS_AND_BATTLEFRAMES.md) for how to edit `characters.json`, change battleframes, add custom characters, configure visuals, and understand the GUID/zone scheme.

### GameServer config

`GameServer.config.json` sits next to `GameServer.exe` and holds the Firefall
installation paths:

```json
{
  "StaticDBPath": "C:\\Program Files\\Steam\\steamapps\\common\\Firefall\\system\\db\\clientdb.sd2",
  "MapsPath": "C:\\Program Files\\Steam\\steamapps\\common\\Firefall\\system\\maps",
  "AssetDBPath": "C:\\Program Files\\Steam\\steamapps\\common\\Firefall\\system\\assetdb",
  "CachePath": ""
}
```

`StaticDBPath` is required; the server will not start until it points at a
`clientdb.sd2` that exists. `MapsPath` and `AssetDBPath` are optional for a
minimal zone/collision setup but should point at the matching Firefall folders.

The server scans for a Firefall client installation on startup and writes the
detected paths into `GameServer.config.json` automatically when they are empty.
It looks in the Steam install location (read from the Windows registry, or the
default `Program Files` folders) and in every library listed in
`libraryfolders.vdf`, then in common standalone install folders, and finally
around the server executable/working directory. Any values you have set in the
file are kept as-is, and empty values never override paths configured in
`App.config`. If you want to point the server at a specific copy, set
`PIN_FIREFALL_PATH` to the Firefall install directory (or `PIN_STEAM_PATH` to
the Steam directory) before starting it.

After a local build it lands in
`UdpHosts\GameServer\bin\Release\net10.0\GameServer.config.json`. In the
GitHub release archive it is at the root of the zip (`Publish\`), next to
`GameServer.exe`. `GameServer.config.example.json` is shipped alongside it as
a fallback template.

The remaining settings (`Port`, `ZoneId`, `ClientVersion`, `GrpcChannelAddress`,
the `serilog:` logging keys, ...) still live in the XML `App.config`, which ships
next to `GameServer.exe` as `GameServer.dll.config`. GameServer parses that file
directly from disk, so editing it works the same way in a local build and in the
single-file release build.

### Troubleshooting

**`GameServer terminated: CodeBase is not supported on assemblies loaded from a single-file bundle`**

An outdated release. GameServer used to read `App.config` through
`ConfigurationManager`, which locates its file via `Assembly.CodeBase` - an API
that does not exist inside a single-file executable, so the server died on its
first settings lookup, before it ever opened `GameServer.config.json`. Setting
the Firefall paths by hand therefore changed nothing. Download the latest PIN
release (or build from source); no configuration change is needed.

**`StaticDBPath is not configured ...` / `StaticDB file not found at ...`**

The Firefall installation was not auto-detected, or the configured path is
wrong. Set `StaticDBPath` in `GameServer.config.json` to the full path of
`system\db\clientdb.sd2`, or point `PIN_FIREFALL_PATH` at the install directory.

### firefall.ini

```ini
[Config]
OperatorHost = "localhost:4400"

[FilePaths]
AssetStreamPath = "http://localhost:4401/AssetStream/%ENVMNEMONIC%-%BUILDNUM%/"
VTRemotePath = "http://localhost:4401/vtex/%ENVMNEMONIC%-%BUILDNUM%/static.vtex"

[UI]
PlayIntroMovie = false
```

### Features

- Loading into any zone (WebHostManager)
- Basic character movement, including jetpacks and gliders(in work now, trying to get fixed)
- Switch between battleframes with preconfigured loadouts
- Customize character appearance in NewYou (RIN.WebAPI)
- Call down vehicles and some deployables
- Health, damage, shields, bleedout/death/respawn and fall damage (see `Docs/HEALTH_SYSTEM.md`)
- Projectile combat against spawned NPCs (`Docs/SPAWNING_AND_COMBAT.md`)
- Server side NPC AI: spawned mobs notice you, chase, shoot back, give up when they are dragged too far from their spawn point (see `Docs/NPC_AI.md`)

### Limitations

- NPC AI is a in works: basic agro pathfinding, animations, no projectiles and the per monster SDB behaviour trees are ignored
- Most of the UI doesn't work properly
- Most abilities are not fully working
- Vehicles only have physics if a player is driving it (client-side)
- No Encounters
- No PvP

## Development

1. Install Visual Studio or JetBrains Rider
   - Include the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) component or install it separately
2. Recursive clone the repository `git clone --recurse-submodules https://github.com/kasperfriend/PIN.git`
3. Build the solution
4. Edit `GameServer.config.json` produced by the build in `UdpHosts\GameServer\bin\Release\net10.0` (copy `GameServer.config.example.json` to `GameServer.config.json` if it is missing) and set `StaticDBPath`, `MapsPath`, and `AssetDBPath` to your Firefall installation.
5. Trust self-signed development certificates by running `dotnet dev-certs https --trust`
6. Start multiple targets at once
   - Visual Studio: Create a `Multiple Startup Projects` target that start WebHostManager, GameServer and MatrixServer
   - Rider: Create a `Compound` target that starts WebHostManager, GameServer and MatrixServer
7. Edit the `firefall.ini` located in `steamapps\common\Firefall`
8. Add content from above
9. Start Firefall

### Web Hosts

CatchAll (4499 / 44399) is used for now, until the specific APIs are implemented.

| Host       | HTTP | HTTPS | Catch All |
|------------|------|-------|-----------|
| Operator   | 4400 | 44300 | ❌        |
| WebAsset   | 4401 | 44301 | ✔️        |
| ClientApi  | 4402 | 44302 | ❌        |
| InGame     | 4403 | 44303 | ❌        |
| WebAccount | 4404 | 44304 | ✔️        |
| Frontend   | 4405 | 44305 | ✔️        |
| Store      | 4406 | 44306 | ✔️        |
| Chat       | 4407 | 44307 | ❌        |
| Replay     | 4408 | 44308 | ✔️        |
| Web        | 4409 | 44309 | ✔️        |
| Market     | 4410 | 44310 | ✔️        |
| RedHanded  | 4411 | 44311 | ✔️        |

### UDP Servers

| Host          | UDP   |
|---------------|-------|
| Matrix Server | 25000 |
| Game Server   | 25001 |
