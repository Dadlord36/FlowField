using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public readonly struct SpeedComponent : ISharedComponentData
    {
        [ReadOnly]
        public readonly float speed;

        public SpeedComponent(float inSpeed)
        {
            speed = inSpeed;
        }
    }
}