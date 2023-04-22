namespace GameServer.Data;

public class AbilityModule
{
    public uint SdbID { get; set; }
    public byte SlotIDX { get; set; }

    public static AbilityModule Load(uint id, byte slot)
    {
        return new AbilityModule { SdbID = id, SlotIDX = slot };
    }
}