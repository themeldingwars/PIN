# Changelog

## [Unreleased]

### Changed

- Added individual .bat files for each game service pointing directly to the respective `bin\debug` folder.
- Added configuration for the game server. You can edit `settings.json` in the `config` directory when adding working defaults and add a `settings.development.json` in the same folder for local/individual settings.
  Currently, the only option is the log level.
- Added cli options parsing. Currently, the only option is the log level. Specifying wrong options will not stop the server from starting.
- Added 404 handling to the web server pipeline which prints the contents of 404-producing requests as warnings to see what's missing.
- Changed 'missing MSGid' logging to include the details (1st message) in log level warning instead of verbose.
- Added ClientEventController with a corresponding endpoint to receive the events from the client. Currently, only the client uptime seems to be posted on exit of the game.
- Update documentation regarding the usage of web hosts
- Turn Firefall specific location finder to a common location provider

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