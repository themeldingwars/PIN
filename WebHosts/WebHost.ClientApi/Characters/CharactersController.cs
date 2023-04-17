﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebHost.ClientApi.Characters.Models;

namespace WebHost.ClientApi.Characters;

[ApiController]
public class CharactersController : ControllerBase
{
    private readonly ICharactersRepository _charactersRepository;

    public CharactersController(ICharactersRepository charactersRepository)
    {
        _charactersRepository = charactersRepository;
    }

    [Route("api/v2/characters/list")]
    [HttpGet]
    public CharactersList GetCharactersList()
    {
        return _charactersRepository.GetCharacters();
    }

    [Route("api/v1/characters/{characterId}/data")]
    [HttpGet]
    [Produces("application/json")]
    public object CharacterData(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) { return new { }; }

        CharacterData data = new CharacterData
        {
            Key = "76336_0", // ToDo: Game is pushing this through QueryStrings, why?
            Namespace = "bfAbiMap", // ToDo: Game is pushing this through QueryStrings, why?
            Value = "0,1,2,3" // ToDo: What are these?
        };

        return data;
    }

    [Route("api/v3/characters/{characterId}/inventories/bag")]
    [HttpGet]
    [Produces("application/json")]
    public object InventoriesBag(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) { return new { }; }

        InventoriesBag bag = new InventoriesBag
        {
            Items = new object[] {
                new Items {
                    ItemId = 1536337344062485245,
                    ItemSdbId = 84238,
                    OwnerGuid = ulong.Parse(characterId),
                    TypeCode = 248,
                    Quality = 0,
                    CharacterGuid = ulong.Parse(characterId),
                    BoundToOwner = false,
                    CreatedAt = "2013-09-07T05:10:11+00:00",
                    UpdatedAt = "2013-09-07T05:10:11+00:00"
                },
                new Items {
                    ItemId = 2256965954967900669,
                    ItemSdbId = 79789,
                    OwnerGuid = ulong.Parse(characterId),
                    TypeCode = 248,
                    Quality = 0,
                    CharacterGuid = ulong.Parse(characterId),
                    BoundToOwner = false,
                    CreatedAt = "2013-09-07T05:10:11+00:00",
                    UpdatedAt = "2013-09-07T05:10:11+00:00",
                    CreatorGuid = ulong.Parse(characterId)
                },
                new Items {
                    ItemId = 8885818829252295677,
                    ItemSdbId = 30346,
                    OwnerGuid = ulong.Parse(characterId),
                    TypeCode = 248,
                    Quality = 685,
                    CharacterGuid = ulong.Parse(characterId),
                    BoundToOwner = false,
                    CreatedAt = "2013-09-07T05:10:11+00:00",
                    UpdatedAt = "2013-09-07T05:10:11+00:00",
                    CreatorGuid = ulong.Parse(characterId)
                }
            },
            Resources = new object[] {
                new Resources {
                    ItemSdbId = 78007,
                    OwnerGuid = ulong.Parse(characterId),
                    ResourceType = "0",
                    Quantity = 1
                },
                new Resources {
                    ItemSdbId = 75269,
                    OwnerGuid = ulong.Parse(characterId),
                    ResourceType = "0",
                    Quantity = 1
                }
                ,
                new Resources {
                    ItemSdbId = 77343,
                    OwnerGuid = ulong.Parse(characterId),
                    ResourceType = "0",
                    Quantity = 1
                }
                ,
                new Resources {
                    ItemSdbId = 77344,
                    OwnerGuid = ulong.Parse(characterId),
                    ResourceType = "0",
                    Quantity = 1
                }
            }
        };

        return bag;
    }

    [Route("api/v3/characters/{characterId}/inventories/gear/items")]
    [HttpGet]
    [Produces("application/json")]
    public object InventoriesGearItems(string characterId)
    {
        if (string.IsNullOrEmpty(characterId)) { return new { }; }

        var temp = new object[] {
            new Items {
                ItemId = 815797474160817405,
                ItemSdbId = 83945,
                OwnerGuid = ulong.Parse(characterId),
                TypeCode = 244,
                Quality = 885,
                CharacterGuid = ulong.Parse(characterId),
                BoundToOwner = false,
                CreatedAt = "2013-09-07T05:10:11+00:00",
                UpdatedAt = "2013-09-07T05:10:11+00:00",
                Durability = new Durability {
                    Current = 1000,
                    Pool = 0
                },
                AttributeModifiers = new Dictionary<uint, double>() {
                    { 950, 0.0 },
                    { 951, -174.84620344827584 },
                    { 952, -42.6656 },
                    { 1072, 125.0 }
                }
            },
            new Items {
                ItemId = 815797474160966141,
                ItemSdbId = 82924,
                OwnerGuid = ulong.Parse(characterId),
                TypeCode = 244,
                Quality = 159,
                CharacterGuid = ulong.Parse(characterId),
                BoundToOwner = false,
                CreatedAt = "2013-09-07T05:10:11+00:00",
                UpdatedAt = "2013-09-07T05:10:11+00:00",
                Durability = new Durability {
                    Current = 1000,
                    Pool = 0
                },
                AttributeModifiers = new Dictionary<uint, double>() {
                    { 23, 2.4827167 },
                    { 952, -42.45361210150184 },
                    { 950, 0.0 },
                    { 951, -63.9984 },
                    { 956, 6.0 },
                    { 954, 153.0 }
                }
            }
        };
        return temp;
    }
}

public class InventoriesBag
{
    public Array Items { get; set; }
    public Array Resources { get; set; }
}

public class Items
{
    public ulong ItemId { get; set; }
    public uint ItemSdbId { get; set; }
    public ulong OwnerGuid { get; set; }
    public uint TypeCode { get; set; }
    public uint Quality { get; set; }
    public ulong CharacterGuid { get; set; }
    public bool BoundToOwner { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set;}
    public Durability Durability { get; set; }
    public Dictionary<uint, double> AttributeModifiers { get; set; }
    public ulong CreatorGuid { get; set; }
}

public class Resources
{
    public uint ItemSdbId { get; set; }
    public ulong OwnerGuid { get; set; }
    public string ResourceType { get; set; }
    public uint Quantity { get; set; }
}

public class Durability
{
    public uint Current { get; set; }
    public uint Pool { get; set; }
}