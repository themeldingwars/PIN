# Server content

Everything the game server places in the world that the 1962 client never shipped, because Red 5's servers kept it: deployables, outposts, melding perimeters, encounters, NPC spawns and so on. It's an overlay on the client's static DB (SDB) and refers to it by SDB ids.

## Layout

```
schemas/<kind>.schema.json   one JSON schema per kind of content
schemas/common.schema.json   data types the schemas share, e.g. vector3, quaternion and id
spawn-tables/<family>.json   spawn tables, shared by every zone, e.g. brontodons.json
zones/<zone id>/<kind>.json  what a zone has of that kind, a missing file means none
zones/<zone id>/zone.json    settings of the whole zone, a missing file or field means the defaults
```

The kinds so far:

- `deployables`: deployables that are there when the zone starts
- `outposts`: outposts with their spawn points
- `meldings`: melding perimeters
- `encounters`: what runs in the zone, LGV races and melding repulsors so far, the `type` field says which
- `regions`: where NPCs spawn, each with a shape (a circle or a polygon), a spawn table and how many groups it keeps alive

A spawn table lists groups with a weight, e.g. a herd of a Herdmaster and three to five Brontodons. A spawn picks one group and spawns all of its members, the group respawns once all of them are dead. Table ids are unique across the files in `spawn-tables`, since regions refer to tables by id only. A group's `style` says how its members behave together: a `herd` shares a home and drifts around it, `solo` members each keep the spot they spawned at. `spread` is how far they spread around their home, the style's default when left out.

`zone.json` holds the defaults of the zone's regions under `population`: `activation_radius`, how close a player has to come to a region's edge for it to fill (300 m), and `despawn_after_seconds`, how long it keeps its NPCs after the last player left (180 s). A region can override both. Everything else about spawning, the tick rate and how members find a spot, stays in the code.

## Rules

- **Ids** are lowercase letters and digits with dashes, e.g. `ne-brontodon-01`, and unique within their file. Where the game itself uses a number, the entry keeps it, outposts for example, since the client and RIN refer to them by id.
- **Vectors** are `[x, y, z]`, orientations are quaternions `[x, y, z, w]`.
- **Ranges** such as a count or the respawn time are `[min, max]` with both ends included, or a single number.
- **`notes`** instead of comments. The files are written by tools (the Shared.Content library, the replay import, Anvil), and those don't keep comments.
- **`source`** says where an imported entry came from, e.g. a replay or capture entity. Set **`edited`** to `true` when you change an imported entry, a re-import then leaves it alone.
- The schemas make Rider and VS Code complete fields and flag mistakes while you type, through the `$schema` at the top of every file.

## Trying changes

Set `WatchContent` to `true` and `ContentPath` to this folder in the `GameServer` section of `appsettings.json`, then the game server reloads a zone whenever one of its files changes. Without `ContentPath` it reads the copy in the build output, which doesn't see your edits. Deployables, regions, spawn tables and the population settings in `zone.json` change in the running game right away, the other kinds only after a restart. A file with problems is logged and the zone keeps what it had.

## Checking

`dotnet test` in the repository loads and validates every zone.
