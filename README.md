# PIN - Pirate Intelligence Network

The Accord may think their Shared Intelligence Network is unique and impenetrable, but not everyone agrees with their restricted access and constant surveillance. That's why PIN, the Pirate Intelligence Network, has been created.

*Fight the Accord - Kill the Chosen*

https://user-images.githubusercontent.com/920861/134824107-03e9f99c-b420-47c7-b742-efe68967161c.mp4

## Usage

**Note:** If you want to play around with the configuration, see the Development section below

PIN only runs the Matrix and game server (i.e. the UDP protocol).
Everything the client does over HTTP, from the login to the character list and the zone settings, is served by [RIN.WebAPI](https://github.com/themeldingwars/RIN.WebAPI).
You *need* both PIN and RIN.WebAPI!

The easiest way to get the whole stack running is [Calldown](https://github.com/themeldingwars/Calldown), which starts the database, RIN, PIN and the client with a single `aspire run`. If you'd rather set it up by hand:

1. Install Firefall via Steam (paste `steam://install/227700` into address bar of web browser)
2. Set up and start RIN.WebAPI and RIN.InternalAPI as described in the [RIN.WebAPI README](https://github.com/themeldingwars/RIN.WebAPI#readme)
3. Edit the `firefall.ini` located in `steamapps\common\Firefall`
4. Add content from below
5. Download the [latest PIN release](https://github.com/themeldingwars/PIN/releases/latest)
6. Make a backup copy of the original `FirefallClient.exe` in `Firefall\system\bin`
7. Replace the `FirefallClient.exe` with the patched `FirefallClient.exe` from the PIN release
8. Make sure the [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) is installed
9. Start the GameServer and the MatrixServer, e.g. with `Start.cmd`
10. Start Firefall
11. Login with your RIN account
12. Load into the game by pressing the "Enter World" button

### firefall.ini

```ini
[Config]
OperatorHost = "https://localhost:5001"

[UI]
PlayIntroMovie = false
```

### Features

- Loading into any zone
- Basic character movement, including jetpacks and gliders
- Switch between battleframes with preconfigured loadouts
- Customize character appearance in NewYou (RIN.WebAPI)
- Call down vehicles and some deployables

### Limitations

- There is no combat, projectile or damage simulation
- Most of the UI doesn't work properly
- Most abilities are not fully working
- Vehicles only have physics if a player is driving it (client-side)
- No AI
- No Encounters
- No PvP

## Development

1. Install the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Recursive clone the repository `git clone --recurse-submodules https://github.com/themeldingwars/PIN.git`
3. Build the solution
4. Check that `StaticDBPath`, `AssetDBPath` and `MapsPath` in `UdpHosts\GameServer\appsettings.json` point to your Firefall install. To keep your paths out of git, put the overrides into an `appsettings.Local.json` next to it instead
5. Set up and start RIN.WebAPI and RIN.InternalAPI, see the [RIN.WebAPI README](https://github.com/themeldingwars/RIN.WebAPI#readme)
6. Start multiple targets at once
   - Visual Studio: Create a `Multiple Startup Projects` target that start GameServer and MatrixServer
   - Rider: Create a `Compound` target that starts GameServer and MatrixServer
7. Edit the `firefall.ini` located in `steamapps\common\Firefall`
8. Add content from above
9. Start Firefall

Calldown works for development as well, it builds and runs PIN and RIN.WebAPI from the checkouts next to it.

### Web API

The web tier lives in [RIN.WebAPI](https://github.com/themeldingwars/RIN.WebAPI), PIN doesn't serve any HTTP endpoints for the client.
The GameServer fetches the character data from RIN.InternalAPI over gRPC (`GrpcChannelAddress`, `http://localhost:5201` by default). If it can't be reached, the GameServer falls back to hardcoded character data.

| Host             | Port              |
|------------------|-------------------|
| RIN.WebAPI       | 5000 / 5001 (TCP) |
| RIN.InternalAPI  | 5201 (TCP)        |

### UDP Servers

| Host          | UDP   |
|---------------|-------|
| Matrix Server | 25000 |
| Game Server   | 25001 |