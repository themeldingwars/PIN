namespace GameServer.Data;

public class Character
{
    // TODO: Stats and Visuals
    protected Character()
    {
        LoadoutGUID = 184538131u;

        FactionID = 1;
        ArmyGUID = 1u;

        CharVisuals = new CharacterVisuals();
        ChassisVisuals = new ChassisVisuals();
        Loadout = Loadout.Load(LoadoutGUID);
        Faction = Faction.Load(FactionID);
        Army = Army.Load(ArmyGUID);

        Name = "Fallback";
        Gender = CharacterGender.Male;
        Race = CharacterRace.Human;
        CharInfoID = 1;
        NameLocalizationID = 0;
        Level = 45;
        EffectiveLevel = 30;
        JumpJetEnergy = 500f;
        MaxJumpJetEnergy = 500;
        JumpJetRecharge = 156.414169f;
        MaxHealth = 19192;

        VoiceSet = 1000;
        TitleID = 135;

        MaxInventorySlots = 225;
    }

    // Basics / Vitals
    public ulong CharacterGUID { get; set; }
    public string Name { get; set; }
    public CharacterGender Gender { get; set; }
    public CharacterRace Race { get; set; }
    public uint CharInfoID { get; set; }
    public uint NameLocalizationID { get; set; }
    public byte Level { get; set; }
    public byte EffectiveLevel { get; set; }
    public float JumpJetEnergy { get; set; }
    public uint MaxJumpJetEnergy { get; set; }
    public float JumpJetRecharge { get; set; }
    public uint MaxHealth { get; set; }

    // Looks / Voice
    public uint VoiceSet { get; set; }
    public ushort TitleID { get; set; }
    public CharacterVisuals CharVisuals { get; protected set; }
    public ChassisVisuals ChassisVisuals { get; protected set; }

    // Inventory
    public uint MaxInventorySlots { get; set; }
    public uint LoadoutGUID { get; set; }
    public Loadout Loadout { get; set; }

    // Extras / Other
    public byte FactionID { get; set; }
    public Faction Faction { get; set; }
    public ulong ArmyGUID { get; set; }
    public Army Army { get; set; }

    public static Character Load(ulong charID)
    {
        // TODO: Pull from database or w/e
        return new Character { CharacterGUID = charID };
    }
}