namespace GameServer.Data
{
    public class Army
    {
        public ulong GUID { get; set; }
        public string Name { get; set; }

        public static Army Load(ulong guid)
        {
            return new Army { GUID = guid, Name = "[ARMY]" };
        }
    }
}