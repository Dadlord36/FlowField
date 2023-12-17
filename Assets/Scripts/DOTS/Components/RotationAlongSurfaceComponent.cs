using Unity.Entities;

namespace DOTS.Components
{
    public struct RotationAlongSurfaceComponent : IComponentData
    {
        public float angleInDegrees;
    }
}