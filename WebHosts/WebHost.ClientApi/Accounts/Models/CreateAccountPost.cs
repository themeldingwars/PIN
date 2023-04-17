using System.Text.Json.Serialization;

namespace WebHost.ClientApi.Accounts.Models;

public class CreateAccountPost
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("confirm_email")]
    public string ConfirmEmail { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("confirm_password")]
    public string ConfirmPassword { get; set; }

    [JsonPropertyName("birthday")]
    public string Birthday { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("email_optin")]
    public bool EmailOptIn { get; set; }

    [JsonPropertyName("referral_key")]
    public string ReferralKey { get; set; }
}