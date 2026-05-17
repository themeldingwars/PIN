using System.Collections.Generic;
using System.Text.Json.Serialization;
using GameServer.Physics.TagfileLoader;
using Serilog;

namespace GameServer.Physics.ZoneLoader;

public class ENWFData
{
    private static readonly ILogger _logger = Log.ForContext<ENWFData>();

    public class ENWFLayer : ITagfileExternalStorage
    {
        public ulong Id;
        public uint NumPhysicsMatIds;
        public uint[] PhysicsMatIds;
        public uint NumVertBlocks;
        public uint NumIndiceBlocks;
        public uint NumMatItems;
        public uint NumMoppBlocks;

        public VertBlockContent[] VertBlocks { get; set; }
        public IndiceBlockContent[] IndiceBlocks { get; set; }

        [JsonConverter(typeof(TagfileObjectDictionaryConverter))]
        public Dictionary<string, BaseTagfileObject> TagfileObjects { get; set; }

        public BaseTagfileObject GetTagfileObject(string query)
        {
            TagfileObjects.TryGetValue(query, out BaseTagfileObject result);

            if (result != null)
            {
                return result;
            }

            _logger.Warning("Failed to find TagfileObject with query {Query}", query);
            return null;
        }
    }
}