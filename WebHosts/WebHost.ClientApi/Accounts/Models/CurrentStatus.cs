namespace WebHost.ClientApi.Accounts.Models;

public class CurrentStatus
{
    public bool IsActive { get; set; }
    public bool CanLogin { get; set; }
    public bool IsDev { get; set; }
    public bool IsBanned { get; set; }
}