using System.Collections.Generic;

namespace MyGameServer.Data
{
    public class CharacterVisuals : CommonVisuals
    {
        public CharacterVisuals()
        {
            HeadAccessories = new List<uint>();

            HeadMain = 10002;
            Eyes = 0;
            CharTypeID = 0;

            HeadAccessories.Add(10089);
            HeadAccessories.Add(10106);

            Colors.Add(0x52680000u);
            Colors.Add(0x6a2440e0u);
            Colors.Add(0xffff0000u);
            Colors.Add(0x320D0021u);
            Colors.Add(0x320D0021u);

            OrnamentGroups.Add(10224);
            OrnamentGroups.Add(10270);
            OrnamentGroups.Add(10061);
        }

        public uint HeadMain { get; set; }
        public uint Eyes { get; set; }
        public IList<uint> HeadAccessories { get; protected set; }
        public uint CharTypeID { get; set; }

        public static CharacterVisuals Load(ulong charID)
        {
            return new CharacterVisuals();
        }
    }
}