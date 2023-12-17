using Unity.Burst;
using Unity.Entities;

namespace DOTS.Components
{
    [BurstCompile]
    public readonly struct MovementParametersComponent : IComponentData
    {
        public readonly float maxSpeed;
        public readonly float crowdAvoidanceDistance;

        public MovementParametersComponent(float inMaxSpeed, float crowdAvoidanceDistance)
        {
            maxSpeed = inMaxSpeed;
            this.crowdAvoidanceDistance = crowdAvoidanceDistance;
        }
    }
}