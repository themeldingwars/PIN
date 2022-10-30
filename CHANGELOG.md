# Changelog

## [Unreleased]

### Changed

- Added configuration for the game server. You can edit the settings.json for working defaults and settings.development.json for local settings.
  Currently, the only option is the log level
- Added cli options parsing. Currently, the only option is the log level.
- Added 404 handling to the web server pipeline which prints the contents of requests which produce 404 responses as warnings.
- Changed missing MSGid parser logging to include the first message in log level warning as well
- Added ClientEventController with a corresponding endpoint to receive the respective events from the client
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