using System.Runtime.InteropServices;
using DOTS.Components;
using DOTS.Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [StructLayout(LayoutKind.Auto)]
    // [DisableAutoCreation]
    public partial struct VelocityDrivenMovementSystem : ISystem
    {
        private VelocityDrivenMovementJob velocityDrivenMovementJob;

        [BurstCompile]
        [StructLayout(LayoutKind.Auto)]
        private partial struct VelocityDrivenMovementJob : IJobEntity
        {
            [ReadOnly] public float deltaTime;

            private void Execute(RefRO<VelocityComponent> velocityComponent, RefRO<MovementSpeedComponent> movementSpeedComponent,
                RefRW<LocalToWorld> localToWorld)
            {
                ref readonly LocalToWorld currentTransform = ref localToWorld.ValueRO;
                float3 newPosition = currentTransform.Position +
                                     velocityComponent.ValueRO.velocity * movementSpeedComponent.ValueRO.speed * deltaTime;

                localToWorld.ValueRW.Value = float4x4.TRS(newPosition, currentTransform.Rotation, Float3One);
            }
        }

        private static readonly float3 Float3One = new(1f, 1f, 1f);

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MovementSpeedComponent>();
            state.RequireForUpdate<VelocityComponent>();
            state.RequireForUpdate<LocalToWorld>();
            state.RequireForUpdate<VelocityDrivenTag>();

            velocityDrivenMovementJob = new VelocityDrivenMovementJob();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            velocityDrivenMovementJob.deltaTime = SystemAPI.Time.DeltaTime;
            state.Dependency = velocityDrivenMovementJob.ScheduleParallel(state.Dependency);
            state.CompleteDependency();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}