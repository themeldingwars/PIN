# PIN - Pirate Intelligence Network

The Accord may think their Shared Intelligence Network is unique and impenetrable, but not everyone agrees with their restricted access and constant surveillance. That's why PIN, the Pirate Intelligence Network, has been created.

*Fight the Accord - Kill the Chosen*

https://user-images.githubusercontent.com/920861/134824107-03e9f99c-b420-47c7-b742-efe68967161c.mp4

## Usage

**Note:** If you want to play around with the configuration, see the Development section below

1. Install Firefall via Steam (paste `steam://install/227700` into address bar of web browser)
2. Edit the `firefall.ini` located in `steamapps\common\Firefall`
3. Add content from below
4. Download the [latest PIN release](https://github.com/themeldingwars/PIN/releases/latest)
5. Make a backup copy of the original `FirefallClient.exe` in `Firefall\system\bin`
6. Replace the `FirefallClient.exe` with the patched `FirefallClient.exe` from the PIN release
7. Make sure the [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) is installed
8. Trust self-signed development certificates by running `dotnet dev-certs https --trust`
9. Start all three applications:
   - GameServer
   - MatrixServer
   - WebHostManager
10. Start Firefall
11. Login to the server:
    - If Steam auto login has been enabled, you will directly be navigated to the character selection screen
    - Otherwise, leave the login fields blank or enter anything you want and click "Login"
12. Load into the game by pressing the "Enter World" button

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

1. Install Visual Studio or JetBrains Rider
   - Include the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) component or install it separately
2. Recursive clone the repository `git clone --recurse-submodules https://github.com/themeldingwars/PIN.git`
3. Build the solution
4. Edit the `GameServer.dll.config` produced by the build in `UdpHosts\GameServer\bin\Release\net8.0` to ensure that `StaticDBPath` is correct.
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