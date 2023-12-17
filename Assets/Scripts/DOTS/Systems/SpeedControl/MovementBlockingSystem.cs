using DOTS.Buffers;
using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Systems.SpeedControl
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(MovementUnblockingSystem))]
    [DisableAutoCreation]
    public partial struct MovementBlockingSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MovementIsBlockedTag>();
            state.RequireForUpdate<MovementSpeedComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AvoidanceSpeedControlJob(0f)
                .ScheduleParallel(SystemAPI.QueryBuilder().WithAll<MovementIsBlockedTag>().WithAll<MovementSpeedComponent>().Build(),
                    state.Dependency).Complete();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}