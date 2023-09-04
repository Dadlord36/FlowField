using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Components
{
    [BurstCompile]
    public struct VelocityComponent : IComponentData
    {
        public float3 velocity;
    }
}