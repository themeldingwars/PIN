# Server content

Everything the game server places in the world that the 1962 client never shipped, because Red 5's servers kept it: deployables, outposts, melding perimeters, encounters and so on. It's an overlay on the client's static DB (SDB) and refers to it by SDB ids.

## Layout

```
schemas/<kind>.schema.json   one JSON schema per kind of content
schemas/common.schema.json   data types the schemas share, e.g. vector3, quaternion and id
zones/<zone id>/<kind>.json  what a zone has of that kind, a missing file means none
```

The kinds so far:

- `deployables`: deployables that are there when the zone starts
- `outposts`: outposts with their spawn points
- `meldings`: melding perimeters
- `encounters`: what runs in the zone, LGV races and melding repulsors so far, the `type` field says which

## Rules

- **Ids** are lowercase letters and digits with dashes, e.g. `ne-brontodon-01`, and unique within their file. Where the game itself uses a number, the entry keeps it, outposts for example, since the client and RIN refer to them by id.
- **Vectors** are `[x, y, z]`, orientations are quaternions `[x, y, z, w]`.
- **`notes`** instead of comments. The files are written by tools (the Shared.Content library, the replay import, Anvil), and those don't keep comments.
- **`source`** says where an imported entry came from, e.g. a replay or capture entity. Set **`edited`** to `true` when you change an imported entry, a re-import then leaves it alone.
- The schemas make Rider and VS Code complete fields and flag mistakes while you type, through the `$schema` at the top of every file.

## Checking

`dotnet test` in the repository loads and validates every zone.
