using Newtonsoft.Json;
using Unity.Mathematics;

namespace Structs
{
    [JsonObject]
    public struct FlowFieldMap
    {
        public int width;
        public int height;
        public float2[] flowMap;
    }
}