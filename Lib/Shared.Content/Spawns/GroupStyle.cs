using System.Text.Json.Serialization;

namespace Shared.Content.Spawns;

/// <summary>How the members of a group behave together, the game server has the defaults of each style.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GroupStyle>))]
public enum GroupStyle
{
    /// <summary>The members share a home and drift around it together, e.g. Brontodons.</summary>
    [JsonStringEnumMemberName("herd")]
    Herd,

    /// <summary>Every member has its own home where it spawned and minds its own business, e.g. Aranha in the grass.</summary>
    [JsonStringEnumMemberName("solo")]
    Solo,
}
