using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WebHost.ClientApi.Login.Models;

namespace WebHost.ClientApi.Login;

[ApiController]
public class LoginController : ControllerBase
{
    [Route("api/v1/login_alerts")]
    [HttpGet]
    public HttpResponseMessage GetLoginAlerts()
    {
        const string Html = @"<html>
<head>
<meta charset=""utf-8"" />
<title>InGame</title>
</head>
<body>
<strong>Welcome to PIN</strong><br>
The Pirate Intelligence Network is here to serve you all the content you'd want
</body>
</html>";

        return new HttpResponseMessage { Content = new StringContent(Html, Encoding.UTF8, "text/html") };
    }

    [Route("api/v3/characters/{characterId}/login_streak")]
    [HttpGet]
    public object GetLoginStreak(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return new { };
        }

        var loginStreak = new LoginStreak
                          {
                              Id = 290248022,
                              CharacterGuid = ulong.Parse(characterId),
                              Streak = 1,
                              FirstLogin = "2015-05-02",
                              LastUpdated = "2015-05-02",
                              LootResult = "[{\"item_type\":93675,\"quantity\":1}]"
                          };

        return loginStreak;
    }

    [Route("api/v3/daily_rewards")]
    [HttpGet]
    public object GetDailyRewards()
    {
        var dailyRewards = new ConcurrentDictionary<uint, DailyRewards>();
        dailyRewards.AddOrUpdate(1, new DailyRewards { ItemType = 93675, Quantity = 1 }, (k, nc) => nc);
        dailyRewards.AddOrUpdate(2, new DailyRewards { ItemType = 96772, Quantity = 1 }, (k, nc) => nc);
        dailyRewards.AddOrUpdate(3, new DailyRewards { ItemType = 96771, Quantity = 1 }, (k, nc) => nc);
        dailyRewards.AddOrUpdate(4, new DailyRewards { ItemType = 96770, Quantity = 1 }, (k, nc) => nc);
        dailyRewards.AddOrUpdate(5, new DailyRewards { ItemType = 96769, Quantity = 1 }, (k, nc) => nc);

        return dailyRewards;
    }
}