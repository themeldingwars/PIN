namespace WebHost.ClientApi.Characters.Models;

public class CharacterCreate
{
    public string Environment { get; set; }
    public int StartClassId { get; set; }
    public int VoiceSet { get; set; }
    public int EyeColorId { get; set; }
    public int SkinColorId { get; set; }
    public int HeadAccessoryA { get; set; }
    public int HairColorId { get; set; }
    public int Head { get; set; }
    public string Gender { get; set; }
    public bool IsDev { get; set; }
    public string Name { get; set; }
}