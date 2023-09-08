using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public struct CylinderSurfacePositioningComponent : IComponentData
    {
        public float angle;
        public float height;
    }
}