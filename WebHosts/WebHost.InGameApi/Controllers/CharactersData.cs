using Microsoft.AspNetCore.Mvc;

namespace WebHost.InGameApi.Controllers;

[ApiController]
public class CharactersData
{
    
    [Route("character/data")]
    [Route("api/v1/character/data")]
    [HttpGet]
    [Produces("application/json")]
    public static object Data()
    {
        Data data = new Data()
        {
            CharacterGuid = 0x99aabbccddee0000 + 448,
            Name = "Sleepwalker",
            Redbux = 1094,
            Crystite = 4104594,
            Gender = 0,
            UniqueName = "SLEEPWALKER",
            Race = 0,
        };

        return data;
    }

    [Route("api/v1/character_sheet.json")]
    [HttpGet]
    [Produces("application/json")]
    public object CharacterSheet()
    {
        CharacterSheet sheet = new CharacterSheet() {
            Battleframe = new Battleframe()
            {
                ItemSdbId = 76332,
                Name = "Astrek \"Rhino\"",
                WebIcon = "Rhino",
                Constraints = new Constraints() {
                    Mass = new MassPowerCpu() {
                        Level = new LevelValue() {
                            Total = 10,
                            Current = 4
                        },
                        Value = new LevelValue() {
                            Total = 1160,
                            Current = 0
                        }
                    },
                    Power = new MassPowerCpu() {
                        Level = new LevelValue() {
                            Total = 10,
                            Current = 4
                        },
                        Value = new LevelValue() {
                            Total = 580,
                            Current = 0
                        }
                    },
                    Cpu = new MassPowerCpu() {
                        Level = new LevelValue() {
                            Total = 10,
                            Current = 4
                        },
                        Value = new LevelValue() {
                            Total = 12,
                            Current = 0
                        }
                    }
                },
                Xp = new Xp() {
                    CurrentXp = 477962,
                    LifetimeXp = 731962
                }
            }
        };

        return sheet;
    }

    [Route("api/v1/character_sheet/equipped_items.json")]
    [HttpGet]
    [Produces("application/json")]
    public object EquippedItems()
    {
        Equipped equipped = new Equipped()
        {
            Primary = new Item {
                ItemId = "",
                DefaultItemSdbId = 78324,
                IsUnlocked = true
            },
            Secondary = new Item {
                ItemId = "",
                DefaultItemSdbId = 78043,
                IsUnlocked = true
            },
            Ability1 = new Item {
                ItemId = "",
                DefaultItemSdbId = 78326,
                IsUnlocked = true
            },
            Ability2 = new Item {
                ItemId = "",
                DefaultItemSdbId = 78328,
                IsUnlocked = true
            },
            Ability3 = new Item {
                ItemId = "",
                DefaultItemSdbId = 78330,
                IsUnlocked = true
            },
            Hkm = new Item {
                ItemId = "9186949129711219709",
                DefaultItemSdbId = 0,
                IsUnlocked = true
            },
            Passive = new Item {
                ItemId = "9190664271895347709",
                DefaultItemSdbId = 78334,
                IsUnlocked = true
            },
            Jumpjets = new Item {
                 ItemId = "",
                 DefaultItemSdbId = 78070,
                 IsUnlocked = true
            },
            Servos = new Item {
                ItemId = "",
                DefaultItemSdbId = 78068,
                IsUnlocked = true
            },
            Backpack = new Item {
                ItemId = "",
                DefaultItemSdbId = 76018,
                IsUnlocked = true
            },
            Plating = new Item {
                ItemId = "", 
                DefaultItemSdbId = 85203, 
                IsUnlocked = true
            }
        };

        return equipped;
    }
}

public class Data
{
    public ulong CharacterGuid { get; set; }
    public string Name { get; set; }
    public uint Redbux { get; set; }
    public uint Crystite { get; set; }
    public uint Gender { get; set; }
    public string UniqueName { get; set; }
    public uint Race { get; set; }
}

public class Equipped
{
    public Item Primary { get; set; }
    public Item Secondary { get; set; }
    public Item Ability1 { get; set; }
    public Item Ability2 { get; set; }
    public Item Ability3 { get; set; }
    public Item Hkm { get; set; }
    public Item Passive { get; set; }
    public Item Jumpjets { get; set; }
    public Item Servos { get; set; }
    public Item Backpack { get; set; }
    public Item Plating { get; set; }
}

public class Item
{
    public string ItemId { get; set; }
    public uint DefaultItemSdbId { get; set; }
    public bool IsUnlocked { get; set; }
}

public class CharacterSheet
{
    public Battleframe Battleframe { get; set; }
}

public class Battleframe
{
    public uint ItemSdbId { get; set; }
    public string Name { get; set; }
    public string WebIcon { get; set; }
    public Constraints Constraints { get; set; }
    public Xp Xp { get; set; }
}

public class Constraints
{
    public MassPowerCpu Mass { get; set; }
    public MassPowerCpu Power { get; set; }
    public MassPowerCpu Cpu { get; set; }
}

public class Xp
{
    public uint CurrentXp { get; set; }
    public uint LifetimeXp { get; set; }
}

public class MassPowerCpu
{
    public LevelValue Level { get; set; }
    public LevelValue Value { get; set; }
}

public class LevelValue
{
    public uint Total { get; set; }
    public uint Current { get; set; }
}