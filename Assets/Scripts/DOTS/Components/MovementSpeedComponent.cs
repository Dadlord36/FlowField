using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public struct MovementSpeedComponent : IComponentData
    {
        public float speed;

        public MovementSpeedComponent(float speed)
        {
            this.speed = speed;
        }
    }
}