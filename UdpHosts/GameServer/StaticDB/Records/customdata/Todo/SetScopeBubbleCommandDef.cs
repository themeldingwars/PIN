namespace GameServer.StaticDB.Records.customdata;

/// <summary>
/// aptgss::SetScopeBubbleCommandDef. Only the id of the row is known to be real, the columns of the table are
/// still to be recovered from the client database, so everything except the id is optional and a row without a
/// layer leaves the character's scope bubble untouched (see SetScopeBubbleCommand).
/// </summary>
public record SetScopeBubbleCommandDef : ICommandDef
{
    public uint Id { get; set; }

    /// <summary>
    /// Scope bubble layer to put the character in while the effect lasts (0 = no scope).
    /// </summary>
    public uint? Layer { get; set; }

    /// <summary>
    /// Second value of ScopeBubbleInfoData, meaning not recovered yet.
    /// </summary>
    public uint? Unk2 { get; set; }
}
