namespace WebHost.ClientApi.Models
{
    public class CaisStatus
    {
        public string State { get; set; }
        public long Duration { get; set; }
        public long ExpiresAt { get; set; }
    }
}