using System.Collections.Generic;

namespace GameServer.StaticDB.Records.customdata;

public record StatusEffectTag
{
    public uint Id { get; set; }
    public HashSet<uint> StatusEffectIds { get; set; }
}