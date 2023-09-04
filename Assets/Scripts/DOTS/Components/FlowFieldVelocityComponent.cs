using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    [BurstCompile]
    public struct FlowFieldVelocityComponent : IComponentData
    {
        public float2 flowVelocity;
    }
}