using System;
using System.Collections.Generic;

namespace WebHost.ClientApi.Models
{
    public class Character
    {
        public ulong CharacterGuid { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public bool IsDev { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TitleId { get; set; }
        public int TimePlayedSecs { get; set; }
        public bool NeedsNameChange { get; set; }
        public int MaxFrameLevel { get; set; }
        public int FrameSdbId { get; set; }
        public int CurrentLevel { get; set; }
        public int Gender { get; set; }
        public string CurrentGender { get; set; }
        public int EliteRank { get; set; }
        public DateTime LastSeenAt { get; set; }
        public Visuals Visuals { get; set; }
        public IEnumerable<Gear> Gear { get; set; }
        public int ExpiresIn { get; set; }
        public string Race { get; set; }
        public IEnumerable<int> Migrations { get; set; }
    }
}