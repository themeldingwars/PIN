namespace WebHost.ClientApi.Login.Models;

public class LoginStreak
{
    public uint Id { get; set; }
    public ulong CharacterGuid { get; set; }
    public uint Streak { get; set; }
    public string FirstLogin { get; set; }
    public string LastUpdated { get; set; }
    public string LootResult { get; set; }
}