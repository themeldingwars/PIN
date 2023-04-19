namespace WebHost.ClientApi.Characters.Models;

public class CharacterCreate
{
    public string environment { get; set; }
    public int startClassId { get; set; }
    public int voiceSet { get; set; }
    public int eyeColorId { get; set; }
    public int skinColorId { get; set; }
    public int headAccessoryA { get; set; }
    public int hairColorId { get; set; }
    public int head { get; set; }
    public string gender { get; set; }
    public bool isDev { get; set; }
    public string name { get; set; }
}
