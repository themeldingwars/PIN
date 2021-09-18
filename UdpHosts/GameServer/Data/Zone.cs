using System.Collections.Generic;
using System.Numerics;

namespace GameServer.Data
{
    public class Zone
    {
        public Zone()
        {
            POIs = new Dictionary<string, Vector3>();
        }

        public uint ID { get; set; }
        public string Name { get; set; }
        public ulong Timestamp { get; set; }
        public Dictionary<string, Vector3> POIs { get; protected set; }
    }
}