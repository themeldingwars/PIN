using GameServer.Enums.GSS.Character;
using Shared.Udp;
using System.Collections.Generic;

namespace GameServer.Packets.GSS.Character.BaseController
{
    [GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Events.CharacterLoaded)]
    public class CharacterLoaded
    {
        [Field] [Length(2)] public IList<byte> UnkBytes;

        public CharacterLoaded()
        {
            UnkBytes = new byte[] { 0x00, 0x00 };
        }
    }
}