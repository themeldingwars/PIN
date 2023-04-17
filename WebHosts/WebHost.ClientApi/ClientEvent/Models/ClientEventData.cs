namespace WebHost.ClientApi.ClientEvent.Models;

/// <summary>
///     Transmitted to the API within a <see cref="ClientEvent" />
/// </summary>
public class ClientEventData
{
    /// <summary>
    ///     (unknown so far, presumably more details about the event)
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     The source of the event within the client (eg. "login")
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The the actual data in form of a JSON string
    /// </summary>
    public string Data { get; set; }
}