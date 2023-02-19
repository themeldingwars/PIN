using System.Collections.Generic;
using WebHost.ClientApi.Models.Base;

namespace WebHost.ClientApi.Characters.Models;

public class Visuals
{
    public int Id { get; set; }
    public int Race { get; set; }
    public int Gender { get; set; }
    public ColoredItem SkinColor { get; set; }
    public Item VoiceSet { get; set; }
    public Item Head { get; set; }
    public ColoredItem EyeColor { get; set; }
    public ColoredItem LipColor { get; set; }
    public ColoredItem HairColor { get; set; }
    public ColoredItem FacialHairColor { get; set; }
    public IEnumerable<ColoredItem> HeadAccessories { get; set; }
    public IEnumerable<ColoredItem> Ornaments { get; set; }
    public Item Eyes { get; set; }
    public HairItem Hair { get; set; }
    public HairItem FacialHair { get; set; }
    public Item Glider { get; set; }
    public Item Vehicle { get; set; }
    public IEnumerable<ColoredTransformableSdbItem> Decals { get; set; }
    public int WarpaintId { get; set; }
    public IEnumerable<long> Warpaint { get; set; }
    public IEnumerable<long> Decalgradients { get; set; }
    public IEnumerable<WarpaintPattern> WarpaintPatterns { get; set; }
    public IEnumerable<long> VisualOverrides { get; set; }
}