using Structs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    [BurstCompile]
    public struct FlowMapComponent : IComponentData
    {
        public NativeArray2D<float2> flowMap;
    }
}