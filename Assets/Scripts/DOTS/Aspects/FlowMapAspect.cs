using DOTS.Components;
using Structs;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Aspects
{
    public readonly partial struct FlowMapAspect : IAspect
    {
        private const string _flowMapFileName = "flow_field";
        
        private readonly RefRW<FlowMapComponent> _flowMap;
        
        public float2 this[int x, int y] => _flowMap.ValueRO.flowMap[x, y];
        
        public NativeArray2D<float2> FlowMap => _flowMap.ValueRO.flowMap;
    }
}