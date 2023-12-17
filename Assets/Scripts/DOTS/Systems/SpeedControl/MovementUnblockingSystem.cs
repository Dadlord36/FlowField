using DOTS.Buffers;
using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Systems.SpeedControl
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CrowdAvoidanceSystem))]
    [DisableAutoCreation]
    public partial struct MovementUnblockingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MovementSpeedComponent>();
            state.RequireForUpdate<MovementIsBlockedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref readonly MovementParametersComponent
                movementParametersComponent = ref SystemAPI.GetSingletonRW<MovementParametersComponent>().ValueRO;

            new AvoidanceSpeedControlJob(movementParametersComponent.maxSpeed).ScheduleParallel(
                SystemAPI.QueryBuilder().WithDisabled<MovementIsBlockedTag>().WithAll<MovementSpeedComponent>().Build(),
                state.Dependency).Complete();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}