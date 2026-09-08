using AeroMessages.GSS.Character;
using GameServer.Entities.Character;

namespace GameServer.Tests.Fakes;

/// <summary>
/// Creates a character whose initial views can be serialized without loading SDB-backed appearance or
/// equipment. The bare constructor leaves those fields for Load/ApplyLoadout; effect application flushes
/// every view, so runtime tests must initialize their strings and nested arrays just like a spawned character.
/// </summary>
public static class FakeCharacterFactory
{
    public static CharacterEntity Create(IShard shard)
    {
        var character = new CharacterEntity(shard, shard.GetNextGuid());
        character.SetStaticInfo(new StaticInfoData
        {
            DisplayName = "Test character",
            UniqueName = "Test character",
            ArmyTag = string.Empty,
            HeadAccessories = [],
            Visuals = EmptyVisuals(),
        });
        character.SetCurrentEquipment(new EquipmentData
        {
            Chassis = EmptyItem(),
            Backpack = EmptyItem(),
            PrimaryWeapon = new SlottedWeapon { Item = EmptyItem() },
            SecondaryWeapon = new SlottedWeapon { Item = EmptyItem() },
        });
        return character;
    }

    private static SlottedItem EmptyItem() => new()
    {
        Modules = [],
        Visuals = EmptyVisuals(),
    };

    private static VisualsBlock EmptyVisuals() => new()
    {
        Decals = [],
        Gradients = [],
        Colors = [],
        Palettes = [],
        Patterns = [],
        OrnamentGroupIds = [],
        CziMapAssetIds = [],
        MorphWeights = [],
        Overlays = [],
    };
}
