namespace GameServer.Aptitude;

/// <summary>
/// Describe how a chain/command is executing for debug purposes.
/// </summary>
public enum ExecutionHint
{
    
    /// <summary>
    /// From an Ability reference
    /// </summary>
    Ability,

    /// <summary>
    /// Through a triggered LocalProximityCommandDef / RemoteProximityCommandDef
    /// </summary>
    Proximity,

    /// <summary>
    /// As the apply chain of an effect
    /// </summary>
    ApplyEffect,

    /// <summary>
    /// As the remove chain of an effect
    /// </summary>
    RemoveEffect,

    /// <summary>
    /// As the duration chain of an effect
    /// </summary>
    DurationEffect,

    /// <summary>
    /// As the update chain of an effect
    /// </summary>
    UpdateEffect,

    /// <summary>
    /// Through a logic related command
    /// </summary>
    Logic,

    /// <summary>
    /// ???
    /// </summary>
    Unspecified,
}