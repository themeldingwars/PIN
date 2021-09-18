# PIN - Pirate Intelligence Network

The Accord may think their Shared Intelligence Network is unique and impenetrable, but not everyone agrees with their restricted access and constant surveillance. That's why PIN, the Pirate Intelligence Network, has been created.

*Fight the Accord - Kill the Chosen*

## Usage

1. Install Firefall via Steam
2. Edit the `firefall.ini` located in `steamapps\common\Firefall`
3. Add content from below
4. Recursive clone the repository `git clone --recurse-submodules https://github.com/themeldingwars/PIN.git`
5. Build the solution
6. Trust self-signed development certificates by running `dotnet dev-certs https --trust`
7. Start multiple targets at once
    - Visual Studio: Create a `Multiple Startup Projects` target that start WebHostManager, GameServer and MatrixServer
    - Rider: Create a `Compound` target that starts WebHostManager, GameServer and MatrixServer
8. Start Firefall

```ini
[Config]
OperatorHost = "localhost:4400"

[FilePaths]
AssetStreamPath = "http://localhost:4401/AssetStream/%ENVMNEMONIC%-%BUILDNUM%/"
VTRemotePath = "http://localhost:4401/vtex/%ENVMNEMONIC%-%BUILDNUM%/static.vtex"

[UI]
PlayIntroMovie = false
```

## Development

### Web Hosts

| Host       | HTTP | HTTPS |
|------------|------|-------|
| Operator   | 4400 | 44300 |
| WebAsset   | 4401 | 44301 |
| ClientApi  | 4402 | 44302 |
| InGame     | 4403 | 44303 |
| WebAccount | 4404 | 44304 |
| Frontend   | 4405 | 44305 |
| Store      | 4406 | 44306 |
| Chat       | 4407 | 44307 |
| Replay     | 4408 | 44308 |
| Web        | 4409 | 44309 |
| Market     | 4410 | 44310 |
| RedHanded  | 4411 | 44311 |

### Game Server

| Host        | UDP   |
|-------------|-------|
| Game Server | 25000 |